using Auth.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Cycles;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeBsc.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeBsc
{
    /// <summary>
    /// Design §6.2 T22 (cycle-level), §8.6 place-before-bsc. The cycle's proposals are placed before the Budget
    /// Sub-Committee once finalisation is complete. Order, each refusal leaving every proposal where it is:
    /// <list type="number">
    /// <item>actor (M-20); right F19.Allocate (M-20); cycle absent → NOT_FOUND; not OPEN → M-17;</item>
    /// <item>no proposal of the cycle at a pre-placement state (S-01…S-10), else M-19 with <c>outstanding[]</c>;</item>
    /// <item>the expected-office check of T20 holds for every category (WORKS → active DIVISION offices;
    /// head-wise → offices with a live T-21 assignment), else M-19 with <c>missingOffices[]</c>;</item>
    /// <item>every S-11 proposal moves to S-12 through <see cref="IProposalTransitionService.MoveAllAsync"/>:
    /// one T-33 row each in one transaction (all-or-nothing).</item>
    /// </list>
    /// A cycle with nothing at S-11 has nothing to place: M-19.
    /// </summary>
    public sealed class PlaceBeforeBscHandler(
        BudgetContext context, IBudgetActorSource actorSource, IAuthorizationService authorization,
        IHttpContextAccessor httpContextAccessor, IProposalTransitionService transitions)
        : IRequestHandler<PlaceBeforeBscCommand, Response>
    {
        // Pre-placement states: a proposal here means finalisation of its category is not complete.
        private static readonly string[] PrePlacement =
        [
            ProposalStateCode.S01, ProposalStateCode.S02, ProposalStateCode.S03, ProposalStateCode.S04,
            ProposalStateCode.S05, ProposalStateCode.S06, ProposalStateCode.S07, ProposalStateCode.S08,
            ProposalStateCode.S09, ProposalStateCode.S10,
        ];

        public async Task<Response> Handle(PlaceBeforeBscCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            if (!(await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy("F19.Allocate"))).Succeeded)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == request.CycleId).Select(c => c.Status).FirstOrDefaultAsync(ct);
            if (cycleStatus is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M17);

            var proposals = await context.BudgetProposals.AsNoTracking()
                .Where(p => p.CycleId == request.CycleId)
                .OrderBy(p => p.ProposalId).ToListAsync(ct);

            var result = new BulkTransitionResponseDto
            {
                Outstanding = proposals.Where(p => PrePlacement.Contains(p.StateCode))
                    .Select(p => new OutstandingProposalDto(p.ProposalId, p.OriginOfficeId, p.StateCode)).ToList(),
            };

            // The expected-office check of T20, applied to every category present in the cycle.
            result.MissingOffices = await PlacementExpectedSet.MissingOfficesAsync(context, request.CycleId, proposals, ct);

            if (result.Outstanding.Count > 0 || result.MissingOffices.Count > 0)
                return BudgetResponses.Fail(BudgetMessages.M19, result);

            var toPlace = proposals.Where(p => p.StateCode == ProposalStateCode.S11)
                .Select(p => p.ProposalId).ToList();
            if (toPlace.Count == 0)
                return BudgetResponses.Fail(BudgetMessages.M19, result);

            var moved = await transitions.MoveAllAsync(toPlace, ProposalAction.PlaceBsc, null, ct);
            if (!moved.IsSucced)
                return moved;
            result.Moved = toPlace;
            return new Response<BulkTransitionResponseDto> { IsSucced = true, Message = "Placed before BSC", Data = result };
        }
    }
}
