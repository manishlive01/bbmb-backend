using Auth.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures
{
    /// <summary>
    /// Design §8.3 AlterFigures, §6.4 reviewer paragraph (BUD-113, BUD-122, BUD-126, BUD-127, BUD-149). Order:
    /// <list type="number">
    /// <item>no actor → M-20; proposal not visible → NOT_FOUND; the actor's office is not the holder → M-20 (§4.9);</item>
    /// <item>state: S-03 / S-06 / S-08 (reviewer, moment ReviewerAlter) or S-10 for WORKS (Finance, moment
    /// FinanceAlter, which skips VR-03/04/05 and keeps VR-15, BUD-317); S-10 non-WORKS → M-19 (F-13 columns 1–8
    /// are closed, BUD-149; DB-11 backstop); any other state → M-19;</item>
    /// <item>the state's right (Review.Examine, or F10.Act at S-10) through IAuthorizationService → M-20;
    /// cycle OPEN (M-17); another officer's live lease (M-23); the line live (NOT_FOUND / M-19);</item>
    /// <item>a figure changed without a justification and without "continue" → <c>ADV-122</c>, nothing saved;
    /// with "continue" the scope records <c>noJustification</c> under action CONTINUE_WITHOUT_JUSTIFICATION;</item>
    /// <item>the rule set over the line in its new form: refusals → <c>Response&lt;RuleFailuresDto&gt;</c>.</item>
    /// </list>
    /// The earlier figure stays in T-31 under its own level (DB-14: the trigger writes the new version with the
    /// holder office, ALTERATION). Finance acts for the FA &amp; CAO (scope on-behalf-of FA_CAO, BUD-127).
    /// No notification is sent (BUD-114). A null figure in the body leaves that figure as it stands.
    /// </summary>
    public sealed class AlterFiguresHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        IDemandLineRules rules, TimeProvider? clock = null)
        : IRequestHandler<AlterFiguresCommand, Response>
    {
        public const string ActionCode = "ALTER_FIGURES";
        public const string ContinueAction = "CONTINUE_WITHOUT_JUSTIFICATION";
        public const string FinancePost = "FA_CAO";
        private static readonly string[] ReviewerStates = [ProposalStateCode.S03, ProposalStateCode.S06, ProposalStateCode.S08];

        public async Task<Response> Handle(AlterFiguresCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, true, ct);
            if (p is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (p.HolderOfficeId != actor.OfficeId)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var atFinance = p.StateCode == ProposalStateCode.S10;
            if (!(ReviewerStates.Contains(p.StateCode) || (atFinance && p.Category == ProposalCategory.Works)))
                return BudgetResponses.Fail(BudgetMessages.M19);
            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            var right = atFinance ? "F10.Act" : "Review.Examine";
            if (!(await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy(right))).Succeeded)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == p.CycleId).Select(c => c.Status).SingleAsync(ct);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M17);
            if (ProposalLease.IsHeldByOther(p, actor.EmployeeId, now))
                return BudgetResponses.Fail(BudgetMessages.M23);
            var (line, lineRefusal) = await DemandLineWrites.FindLiveLineAsync(context, p, request.DemandLineId, true, ct);
            if (lineRefusal is not null)
                return lineRefusal;

            var dto = request.Dto;
            var rbe = dto.RbeAmount ?? line!.RbeAmount;
            var obe = dto.ObeNextAmount ?? line!.ObeNextAmount;
            var changed = rbe != line!.RbeAmount || obe != line.ObeNextAmount;
            var justification = string.IsNullOrWhiteSpace(dto.Justification) ? null : dto.Justification.Trim();
            if (changed && justification is null && !dto.ContinueWithoutJustification)
                return BudgetResponses.Fail(BudgetMessages.Adv122);
            var noJustification = changed && justification is null;

            await using var tx = await scope.BeginAsync(
                noJustification ? ContinueAction : ActionCode, justification, noJustification,
                atFinance ? FinancePost : null, ct);
            line.RbeAmount = rbe;
            line.ObeNextAmount = obe;
            var result = await DemandLineWrites.EvaluateSaveAsync(
                context, rules, p, line, actor.OfficeLevel, ct,
                atFinance ? RuleMoment.FinanceAlter : RuleMoment.ReviewerAlter);
            if (result.Refused)
                return BudgetResponses.RuleFailures(result.ToDto()); // tx disposed uncommitted: nothing written
            DemandLineWrites.RenewLease(p, actor, now, ProposalLease.Length(configuration));
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response<SaveDemandLineResponseDto>
            {
                IsSucced = true, Message = changed ? "Figures altered" : "No figure changed",
                Data = new SaveDemandLineResponseDto(line.DemandLineId, line.SerialNo, [.. result.Warnings]),
            };
        }
    }
}
