using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Rules;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create
{
    /// <summary>
    /// F-06 Submit (design §8.4; BUD-068 to BUD-076, BUD-274). Order:
    /// <list type="number">
    /// <item>Actor (M-20); at a DIVISION office (the DDO of the Division) else M-20.</item>
    /// <item>AA tab: the approval absent or of another Division → NOT_FOUND (§4.9); the work is the approval's
    /// (BUD-069). AMR tab with workId: absent or of another Division → NOT_FOUND; pre_ddd → M-21; class not AMR →
    /// M-22; a sanction already exists → M-06 (one per AM&amp;R work, BUD-274). AMR tab with newAmrWork: code usable
    /// (M-05).</item>
    /// <item>Copy EVIDENCE uploaded by the actor (M-07, VR-10); TS number unused in the Division (DB-07, M-19).</item>
    /// <item>In one TS_CREATE scope: the new AM&amp;R work (<c>entered_via = TS_ENTRY</c>); on the AA tab VR-02 under
    /// the DB-09 lock key (M-06, no override); T-27 with the allotted serial and T-28 v1. DB-09 re-checks at commit.</item>
    /// </list>
    /// Approval and sanction are never recorded in one request (BUD-076): this is a separate endpoint.
    /// </summary>
    public sealed class CreateTechnicalSanctionHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, TechnicalSanctionRules rules, WorkRules workRules)
        : IRequestHandler<CreateTechnicalSanctionCommand, Response<long>>
    {
        public const string ActionCode = "TS_CREATE";
        /// <summary>Legacy NOT NULL <c>work.user_code</c> of a work created with its sanction (as DemandEntryUserCode).</summary>
        public const string TsEntryUserCode = "BUDTS";

        public async Task<Response<long>> Handle(CreateTechnicalSanctionCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            if (await rules.ActorDivisionAsync(actor, ct) is not { } division)
                return BudgetResponses.Fail<long>(BudgetMessages.M20);
            var dto = request.Dto;

            Work? work = null;
            Features.HeadOfAccounts.Entities.HeadOfAccount? newWorkCode = null;
            if (dto.Tab == SanctionTab.Aa)
            {
                var approval = await context.AdminApprovals.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AdminApprovalId == dto.AdminApprovalId, ct);
                if (approval is null || approval.DivisionOfficeId != division.OfficeId)
                    return BudgetResponses.Fail<long>(BudgetMessages.NotFound);
                work = await context.Works.AsNoTracking().SingleAsync(w => w.Id == approval.WorkId, ct);
            }
            else if (dto.WorkId is { } workId)
            {
                work = await context.Works.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workId, ct);
                if (work is null || work.DivisionId != division.DivisionId)
                    return BudgetResponses.Fail<long>(BudgetMessages.NotFound);
                if (work.PreDdd)
                    return BudgetResponses.Fail<long>(BudgetMessages.M21);
                if (work.WorkClass != WorkClass.Amr)
                    return BudgetResponses.Fail<long>(BudgetMessages.M22);
                if (await context.TechnicalSanctions.AsNoTracking().AnyAsync(t => t.WorkId == work.Id, ct))
                    return BudgetResponses.Fail<long>(BudgetMessages.M06);
            }
            else
            {
                var (code, failure) = await workRules.ResolveAccountCodeAsync(dto.NewAmrWork!.AccountCodeId, ct);
                if (code is null)
                    return BudgetResponses.Fail<long>(failure!);
                newWorkCode = code;
            }

            if (!await rules.IsUsableEvidenceAsync(dto.TsCopyAttachmentId!.Value, actor, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M07);
            var tsNumber = dto.TsNumber.Trim();
            if (await rules.IsTsNumberUsedAsync(division.DivisionId, tsNumber, null, ct))
                return BudgetResponses.Fail<long>(BudgetMessages.M19);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            if (dto.Tab == SanctionTab.Aa &&
                await rules.CheckAaCeilingAsync(dto.AdminApprovalId!.Value, dto.TsAmount, null, ct) is { } ceiling)
                return BudgetResponses.Fail<long>(ceiling); // tx disposed uncommitted
            if (work is null)
            {
                work = new Work
                {
                    WorkCode = await workRules.NextInterimWorkCodeAsync(division.DivisionId, ct),
                    DivisionId = division.DivisionId, WorkDesc = dto.NewAmrWork!.NameOfWork.Trim(), WorkClass = WorkClass.Amr,
                    EnteredVia = WorkEnteredVia.TsEntry, PreDdd = false, UserCode = TsEntryUserCode,
                };
                WorkRules.ApplySegments(work, newWorkCode!);
                context.Works.Add(work);
                await context.SaveChangesAsync(ct);
            }
            var header = new TechnicalSanction
            {
                WorkId = work.Id, Tab = dto.Tab, AdminApprovalId = dto.Tab == SanctionTab.Aa ? dto.AdminApprovalId : null,
                SerialNo = await rules.NextSerialAsync(division.DivisionId, ct), CurrentVersionNo = 1,
            };
            context.TechnicalSanctions.Add(header);
            await context.SaveChangesAsync(ct);
            context.TechnicalSanctionVersions.Add(new TechnicalSanctionVersion
            {
                TechnicalSanctionId = header.TechnicalSanctionId, VersionNo = 1, NameOfEstimate = dto.NameOfEstimate.Trim(),
                TsNumber = tsNumber, TsDate = dto.TsDate, TsAmount = dto.TsAmount, TsCopyAttachmentId = dto.TsCopyAttachmentId.Value,
            });
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<long> { IsSucced = true, Message = "Technical sanction recorded", Data = header.TechnicalSanctionId };
        }
    }
}
