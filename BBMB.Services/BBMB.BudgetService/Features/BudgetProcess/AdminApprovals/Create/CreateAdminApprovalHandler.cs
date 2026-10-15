using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Rules;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create
{
    /// <summary>
    /// F-03 Submit (design §8.4; BUD-052 to BUD-059). There is no Save: the approval and its version 1 are
    /// written only here (DDD T-25/T-26). Order:
    /// <list type="number">
    /// <item>Actor (M-20); the actor is at a DIVISION office (BUD-052: never a higher office) else M-20.</item>
    /// <item>workId: absent or of another Division → NOT_FOUND (§4.9: a DDO sees only its own Division);
    /// <c>pre_ddd</c> → M-21 (its class and entry route are not recorded); class not AA → M-22; an approval
    /// already exists → M-19 (one per work; a change is a revision, F-04).</item>
    /// <item>Account code exists and is not NFU (M-05). For an existing work the code is the work's own head
    /// (M-05 otherwise): the demand line takes the work's head (§5.5 row i), and a head change is a revision
    /// (AA_HEAD_CHANGED).</item>
    /// <item>Copy: exists, EVIDENCE, uploaded by the actor (M-04, VR-10).</item>
    /// <item>Reference unused by another approval of the Division (DB-07 pre-check, M-19 under OI-13).</item>
    /// <item>BUD-055: above the delegated power without a justification → M-27 with Data.exceedsDelegation and
    /// nothing saved; with a justification → saved with the flag.</item>
    /// <item>In one AA_CREATE scope: the new work (<c>entered_via = AA_ENTRY</c>, class AA, interim code until
    /// P4), T-25 with the allotted serial, T-26 v1, and the BUD-059 WORK line where the current WORKS demand is
    /// at S-01/S-04.</item>
    /// </list>
    /// </summary>
    public sealed class CreateAdminApprovalHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, AdminApprovalRules rules, WorkRules workRules)
        : IRequestHandler<CreateAdminApprovalCommand, Response<CreateAdminApprovalResponseDto>>
    {
        public const string ActionCode = "AA_CREATE";
        /// <summary>Legacy NOT NULL <c>work.user_code</c> of a work created with its approval (as DemandEntryUserCode).</summary>
        public const string AaEntryUserCode = "BUDAA";

        public async Task<Response<CreateAdminApprovalResponseDto>> Handle(CreateAdminApprovalCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.M20);
            if (await rules.ActorDivisionAsync(actor, ct) is not { } division)
                return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.M20);
            var dto = request.Dto;

            Work? work = null;
            if (dto.WorkId is { } workId)
            {
                work = await context.Works.FirstOrDefaultAsync(w => w.Id == workId, ct);
                if (work is null || work.DivisionId != division.DivisionId)
                    return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.NotFound);
                if (work.PreDdd)
                    return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.M21);
                if (work.WorkClass != WorkClass.Aa)
                    return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.M22);
                if (await context.AdminApprovals.AsNoTracking().AnyAsync(a => a.WorkId == work.Id, ct))
                    return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.M19);
            }

            var (code, codeFailure) = await workRules.ResolveAccountCodeAsync(dto.AccountCodeId, ct);
            if (code is null)
                return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(codeFailure!);
            if (work is not null && !WorkRules.HasSegments(work, code))
                return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.M05);

            if (!await rules.IsUsableEvidenceAsync(dto.AaCopyAttachmentId!.Value, actor, ct))
                return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.M04);
            var reference = dto.AaReferenceNo.Trim();
            if (await rules.IsReferenceUsedAsync(division.OfficeId, reference, null, ct))
                return BudgetResponses.Fail<CreateAdminApprovalResponseDto>(BudgetMessages.M19);

            var exceeds = await rules.CheckDelegationAsync(dto.CompetentAuthority, dto.AmountSanctioned, dto.DateOfApproval, ct);
            var justification = string.IsNullOrWhiteSpace(dto.DelegationJustification) ? null : dto.DelegationJustification.Trim();
            if (exceeds && justification is null)
                return BudgetResponses.Fail(BudgetMessages.M27, new CreateAdminApprovalResponseDto { ExceedsDelegation = true });

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            if (work is null)
            {
                work = new Work
                {
                    WorkCode = await workRules.NextInterimWorkCodeAsync(division.DivisionId, ct),
                    DivisionId = division.DivisionId, WorkDesc = dto.NameOfWork.Trim(), WorkClass = WorkClass.Aa,
                    EnteredVia = WorkEnteredVia.AaEntry, PreDdd = false, UserCode = AaEntryUserCode,
                };
                WorkRules.ApplySegments(work, code);
                context.Works.Add(work);
                await context.SaveChangesAsync(ct);
            }
            var header = new AdminApproval
            {
                WorkId = work.Id, DivisionOfficeId = division.OfficeId, CurrentVersionNo = 1,
                SerialNo = await rules.NextSerialAsync(division.OfficeId, ct),
            };
            context.AdminApprovals.Add(header);
            await context.SaveChangesAsync(ct);
            context.AdminApprovalVersions.Add(new AdminApprovalVersion
            {
                AdminApprovalId = header.AdminApprovalId, VersionNo = 1, NameOfWork = dto.NameOfWork.Trim(),
                AaReferenceNo = reference, DateOfApproval = dto.DateOfApproval, AccountCodeId = code.AccountCodeId,
                AmountSanctioned = dto.AmountSanctioned, AaCopyAttachmentId = dto.AaCopyAttachmentId.Value,
                EofficeRef = string.IsNullOrWhiteSpace(dto.EofficeRef) ? null : dto.EofficeRef.Trim(),
                CompetentAuthority = dto.CompetentAuthority, ExceedsDelegation = exceeds,
                DelegationJustification = exceeds ? justification : null,
            });
            await context.SaveChangesAsync(ct);
            var lineId = await rules.AddWorkLineIfOpenAsync(division.OfficeId, work, code.AccountCodeId, ct);
            await tx.CommitAsync(ct);
            return new Response<CreateAdminApprovalResponseDto>
            {
                IsSucced = true,
                Message = exceeds ? "Approval recorded above the delegated power, with justification" : "Approval recorded",
                Data = new CreateAdminApprovalResponseDto
                {
                    AdminApprovalId = header.AdminApprovalId, SerialNo = header.SerialNo, WorkId = work.Id,
                    ExceedsDelegation = exceeds, DemandLineId = lineId,
                },
            };
        }
    }
}
