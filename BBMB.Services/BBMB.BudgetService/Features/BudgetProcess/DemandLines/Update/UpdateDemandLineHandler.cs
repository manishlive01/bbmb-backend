using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update
{
    /// <summary>
    /// Design §8.3 Update: the origin office at S-01 / S-04 only (else M-19, <see cref="DemandLineWrites"/>).
    /// A line not of the proposal → NOT_FOUND; a line marked deleted → M-19 (it is kept, not edited, BUD-319).
    /// Fields by stored type: HEAD requires accountCodeId (M-21); UNFORESEEN requires unforeseenName (M-21) and
    /// takes no code (M-22); precedingAmrWorkId only for a WORK line of an AMR work, from the options (M-22).
    /// Save moment as Add. A figure change writes T-31 by trigger, <c>change_source = ENTRY</c> (DB-14).
    /// </summary>
    public sealed class UpdateDemandLineHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        IDemandLineRules rules, TimeProvider? clock = null)
        : IRequestHandler<UpdateDemandLineCommand, Response>
    {
        public async Task<Response> Handle(UpdateDemandLineCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var target = await DemandLineWrites.ResolveAsync(
                context, actorSource, officeScope, authorization, httpContextAccessor, request.ProposalId, now, ct);
            if (target.Refusal is { } refusal)
                return refusal;
            var (actor, p) = (target.Actor!, target.Proposal!);
            var dto = request.Dto;

            await using var tx = await scope.BeginAsync(DemandLineWrites.UpdateAction, ct: ct);
            await DemandLineWrites.LockProposalAsync(context, p.ProposalId, ct);
            var line = await context.DemandLines
                .FirstOrDefaultAsync(l => l.DemandLineId == request.DemandLineId && l.ProposalId == p.ProposalId, ct);
            if (line is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (line.IsDeleted)
                return BudgetResponses.Fail(BudgetMessages.M19);

            switch (line.LineType)
            {
                case DemandLineType.Head:
                    if (dto.AccountCodeId is null)
                        return DemandLineWrites.FieldFailure(line, "accountCodeId", BudgetMessages.M21);
                    line.AccountCodeId = dto.AccountCodeId;
                    break;
                case DemandLineType.Unforeseen:
                    if (dto.AccountCodeId is not null)
                        return DemandLineWrites.FieldFailure(line, "accountCodeId", BudgetMessages.M22);
                    if (string.IsNullOrWhiteSpace(dto.UnforeseenName))
                        return DemandLineWrites.FieldFailure(line, "unforeseenName", BudgetMessages.M21);
                    line.UnforeseenName = dto.UnforeseenName.Trim();
                    break;
                case DemandLineType.Work:
                    if (dto.AccountCodeId is { } code)
                        line.AccountCodeId = code; // a code other than the work's fails design §5.5 row i
                    break;
            }
            if (dto.UnforeseenName is not null && line.LineType != DemandLineType.Unforeseen)
                return DemandLineWrites.FieldFailure(line, "unforeseenName", BudgetMessages.M22);
            if (dto.PrecedingAmrWorkId is { } predecessor)
            {
                var work = line.WorkId is { } wid ? await context.Works.FirstOrDefaultAsync(w => w.Id == wid, ct) : null;
                if (work is null || work.WorkClass != WorkClass.Amr ||
                    !(await DemandLineWrites.AmrPredecessorsAsync(context, p, ct)).Any(w => w.Id == predecessor))
                    return DemandLineWrites.FieldFailure(line, "precedingAmrWorkId", BudgetMessages.M22);
                work.PrecedingAmrWorkId = predecessor;
            }
            line.RbeAmount = dto.RbeAmount;
            line.ObeNextAmount = dto.ObeNextAmount;
            line.JustificationText = string.IsNullOrWhiteSpace(dto.JustificationText) ? null : dto.JustificationText.Trim();
            line.JustificationAttachmentId = dto.JustificationAttachmentId;
            line.CeCertificateAttachmentId = dto.CeCertificateAttachmentId;
            line.CeAmrApprovalAttachmentId = dto.CeAmrApprovalAttachmentId;

            var result = await DemandLineWrites.EvaluateSaveAsync(context, rules, p, line, actor.OfficeLevel, ct);
            if (result.Refused)
                return BudgetResponses.RuleFailures(result.ToDto()); // tx disposed uncommitted: nothing written

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
