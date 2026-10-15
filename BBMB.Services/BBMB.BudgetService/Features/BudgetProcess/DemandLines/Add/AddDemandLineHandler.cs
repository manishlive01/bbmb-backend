using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add
{
    /// <summary>
    /// Design §8.3 Add. After the <see cref="DemandLineWrites.ResolveAsync"/> checks, inside one
    /// <c>DEMAND_LINE_ADD</c> scope holding the proposal lock:
    /// <list type="number">
    /// <item>WORK with <c>workId</c>: the work is of the proposal's Division (DB-04), else M-05 on workId; the
    /// line takes the work's resolved code unless one is given (a different one fails design §5.5 row i).</item>
    /// <item>WORK with <c>newWork</c>: the code is usable (M-05); a T-23 work is created with
    /// <c>entered_via = DEMAND_ENTRY</c>, the Division of the origin office and the interim P1 code
    /// (<see cref="WorkRules.NextInterimWorkCodeAsync"/>; code-less from P4, DV-19).</item>
    /// <item><c>precedingAmrWorkId</c>: only on an AMR work, and one of the AM&amp;R predecessor options
    /// (BUD-275), else M-22; set on the work.</item>
    /// <item>Serial max + 1 (BUD-079); <c>added_on_return</c> at S-04 (BUD-320).</item>
    /// <item>Save moment of the line rule set: refusals → <c>Response&lt;RuleFailuresDto&gt;</c> and nothing
    /// is written; warnings are returned with the line (§6.4).</item>
    /// </list>
    /// The write renews the edit lease (§4.10). The T-31 versions are written by trigger (DB-14).
    /// </summary>
    public sealed class AddDemandLineHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        IDemandLineRules rules, WorkRules workRules, TimeProvider? clock = null)
        : IRequestHandler<AddDemandLineCommand, Response>
    {
        public async Task<Response> Handle(AddDemandLineCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var target = await DemandLineWrites.ResolveAsync(
                context, actorSource, officeScope, authorization, httpContextAccessor, request.ProposalId, now, ct);
            if (target.Refusal is { } refusal)
                return refusal;
            var (actor, p) = (target.Actor!, target.Proposal!);
            var dto = request.Dto;
            var line = new DemandLine
            {
                ProposalId = p.ProposalId, LineType = dto.LineType, AccountCodeId = dto.AccountCodeId,
                UnforeseenName = dto.UnforeseenName?.Trim(), RbeAmount = dto.RbeAmount, ObeNextAmount = dto.ObeNextAmount,
                JustificationText = string.IsNullOrWhiteSpace(dto.JustificationText) ? null : dto.JustificationText.Trim(),
                AddedOnReturn = p.StateCode == ProposalStateCode.S04,
                JustificationAttachmentId = dto.JustificationAttachmentId,
                CeCertificateAttachmentId = dto.CeCertificateAttachmentId,
                CeAmrApprovalAttachmentId = dto.CeAmrApprovalAttachmentId,
            };

            await using var tx = await scope.BeginAsync(DemandLineWrites.AddAction, ct: ct);
            await DemandLineWrites.LockProposalAsync(context, p.ProposalId, ct);

            if (dto.LineType == DemandLineType.Work)
            {
                var divisionId = await DemandLineWrites.OriginDivisionIdAsync(context, p, ct);
                Work? work;
                if (dto.WorkId is { } workId)
                {
                    work = await context.Works.FirstOrDefaultAsync(w => w.Id == workId && w.DivisionId == divisionId, ct);
                    if (work is null)
                        return DemandLineWrites.FieldFailure(line, "workId", BudgetMessages.M05);
                    line.AccountCodeId ??= await DemandLineWrites.ResolvedAccountCodeIdAsync(context, work, ct);
                    if (line.AccountCodeId is null)
                        return DemandLineWrites.FieldFailure(line, "accountCodeId", BudgetMessages.M05);
                }
                else
                {
                    var nw = dto.NewWork!;
                    var (code, codeFailure) = await workRules.ResolveAccountCodeAsync(nw.AccountCodeId, ct);
                    if (code is null)
                        return DemandLineWrites.FieldFailure(line, "newWork.accountCodeId", codeFailure!);
                    work = new Work
                    {
                        WorkCode = await workRules.NextInterimWorkCodeAsync(divisionId!.Value, ct),
                        DivisionId = divisionId.Value, WorkDesc = nw.NameOfWork.Trim(), WorkClass = nw.WorkClass,
                        EnteredVia = WorkEnteredVia.DemandEntry, PreDdd = false, UserCode = DemandLineWrites.DemandEntryUserCode,
                    };
                    WorkRules.ApplySegments(work, code);
                    line.AccountCodeId ??= code.AccountCodeId;
                    context.Works.Add(work);
                }
                if (dto.PrecedingAmrWorkId is { } predecessor)
                {
                    if (work.WorkClass != WorkClass.Amr ||
                        !(await DemandLineWrites.AmrPredecessorsAsync(context, p, ct)).Any(w => w.Id == predecessor))
                        return DemandLineWrites.FieldFailure(line, "precedingAmrWorkId", BudgetMessages.M22);
                    work.PrecedingAmrWorkId = predecessor;
                }
                await context.SaveChangesAsync(ct); // the work exists before the rules read it
                line.WorkId = work.Id;
            }

            line.SerialNo = (await context.DemandLines.AsNoTracking()
                .Where(l => l.ProposalId == p.ProposalId).MaxAsync(l => (int?)l.SerialNo, ct) ?? 0) + 1;
            var result = await DemandLineWrites.EvaluateSaveAsync(context, rules, p, line, actor.OfficeLevel, ct);
            if (result.Refused)
                return BudgetResponses.RuleFailures(result.ToDto()); // tx disposed uncommitted: nothing written

            context.DemandLines.Add(line);
            DemandLineWrites.RenewLease(p, actor, now, ProposalLease.Length(configuration));
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<SaveDemandLineResponseDto>
            {
                IsSucced = true, Message = "Demand line saved",
                Data = new SaveDemandLineResponseDto(line.DemandLineId, line.SerialNo, [.. result.Warnings]),
            };
        }
    }
}
