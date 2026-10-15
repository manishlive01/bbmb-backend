using Auth.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory
{
    /// <summary>
    /// Design §6.2 T20 (category-level, BUD-134), §8.3 FinaliseCategory. Order, each refusal leaving every
    /// proposal where it is:
    /// <list type="number">
    /// <item>actor (M-20); the category's right F10.Act (WORKS) / F13.Act (head-wise) (M-20); cycle absent →
    /// NOT_FOUND; not OPEN → M-17;</item>
    /// <item>every proposal of (cycle, category) at S-10 or S-11, else M-19 with <c>outstanding[]</c>;</item>
    /// <item>the expected set re-computed: WORKS → every active DIVISION office (an office activated after the
    /// last open-proposals run is caught here). Head-wise in P1 → the offices named at opening, which are the
    /// proposals themselves (T-21 assignments arrive in P2). A missing office → M-19 with <c>missingOffices[]</c>;</item>
    /// <item>head-wise: every (CE, head) row of V-03 carries a recommendation with a figure (BUD-302), else M-19
    /// with <c>missingRecommendations[]</c>;</item>
    /// <item>moment FinanceFinalise (VR-15, completeness) over the non-deleted lines of every S-10 proposal:
    /// failures → <c>Response&lt;RuleFailuresDto&gt;</c>;</item>
    /// <item>every S-10 proposal moves to S-11 through <see cref="IProposalTransitionService.MoveAllAsync"/>:
    /// one T-33 row each, all in one transaction.</item>
    /// </list>
    /// A category already wholly at S-11 has nothing to finalise: M-19.
    /// </summary>
    public sealed class FinaliseCategoryHandler(
        BudgetContext context, IBudgetActorSource actorSource, IAuthorizationService authorization,
        IHttpContextAccessor httpContextAccessor, IProposalTransitionService transitions, IDemandLineRules rules)
        : IRequestHandler<FinaliseCategoryCommand, Response>
    {
        private static readonly string[] Finalisable = [ProposalStateCode.S10, ProposalStateCode.S11];

        public async Task<Response> Handle(FinaliseCategoryCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var isWorks = request.Category == ProposalCategory.Works;
            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            if (!(await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy(isWorks ? "F10.Act" : "F13.Act"))).Succeeded)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == request.CycleId).Select(c => c.Status).FirstOrDefaultAsync(ct);
            if (cycleStatus is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M17);

            var proposals = await context.BudgetProposals.AsNoTracking()
                .Where(p => p.CycleId == request.CycleId && p.Category == request.Category)
                .OrderBy(p => p.ProposalId).ToListAsync(ct);
            var result = new FinaliseResponseDto();
            result.Outstanding = proposals.Where(p => !Finalisable.Contains(p.StateCode))
                .Select(p => new OutstandingProposalDto(p.ProposalId, p.OriginOfficeId, p.StateCode)).ToList();
            if (isWorks)
            {
                var divisions = await context.BudgetOffices.AsNoTracking()
                    .Where(o => o.IsActive && o.OfficeLevel == OfficeLevel.Division)
                    .OrderBy(o => o.OfficeId).Select(o => o.OfficeId).ToListAsync(ct);
                var origins = proposals.Select(p => p.OriginOfficeId).ToHashSet();
                result.MissingOffices = divisions.Where(d => !origins.Contains(d)).ToList();
            }
            if (result.Outstanding.Count > 0 || result.MissingOffices.Count > 0)
                return BudgetResponses.Fail(BudgetMessages.M19, result);

            if (!isWorks)
            {
                var demanded = await context.DemandByCe.AsNoTracking()
                    .Where(r => r.CycleId == request.CycleId && r.Category == request.Category)
                    .Select(r => new { r.CeOfficeId, r.AccountCodeId }).ToListAsync(ct);
                var recommended = (await context.CeRecommendations.AsNoTracking()
                        .Where(r => r.CycleId == request.CycleId && r.Category == request.Category &&
                                    (r.RbeRecommended != null || r.ObeNextRecommended != null))
                        .Select(r => new { r.CeOfficeId, r.AccountCodeId }).ToListAsync(ct))
                    .ToHashSet();
                result.MissingRecommendations = demanded.Where(d => !recommended.Contains(d))
                    .OrderBy(d => d.CeOfficeId).ThenBy(d => d.AccountCodeId)
                    .Select(d => new MissingRecommendationDto(d.CeOfficeId, d.AccountCodeId)).ToList();
                if (result.MissingRecommendations.Count > 0)
                    return BudgetResponses.Fail(BudgetMessages.M19, result);
            }

            var toFinalise = proposals.Where(p => p.StateCode == ProposalStateCode.S10).ToList();
            if (toFinalise.Count == 0)
                return BudgetResponses.Fail(BudgetMessages.M19, result);
            var failures = new RuleFailuresDto();
            foreach (var p in toFinalise)
            {
                var lines = await context.DemandLines.AsNoTracking()
                    .Where(l => l.ProposalId == p.ProposalId && !l.IsDeleted).OrderBy(l => l.SerialNo).ToListAsync(ct);
                var evaluated = await rules.EvaluateAsync(p, lines, RuleMoment.FinanceFinalise, actor.OfficeLevel, ct);
                failures.Failures.AddRange(evaluated.Failures);
                failures.Warnings.AddRange(evaluated.Warnings);
            }
            if (failures.Failures.Count > 0)
                return BudgetResponses.RuleFailures(failures);

            var moved = await transitions.MoveAllAsync(toFinalise.Select(p => p.ProposalId).ToList(), ProposalAction.Finalise, null, ct);
            if (!moved.IsSucced)
                return moved;
            result.Finalised = toFinalise.Select(p => p.ProposalId).ToList();
            return new Response<FinaliseResponseDto> { IsSucced = true, Message = "Category finalised", Data = result };
        }
    }
}
