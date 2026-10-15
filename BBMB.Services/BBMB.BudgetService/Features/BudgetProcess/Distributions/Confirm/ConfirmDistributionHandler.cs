using BBMB.BudgetService.Features.BudgetProcess.Distributions.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Confirm
{
    /// <summary>
    /// Design §8.5 Confirm (BUD-280, DB-18). Order: actor (M-20) → the recommendation exists in the cycle, else
    /// NOT_FOUND → the actor's office is the recommendation's CE, else M-20 → the current distribution totals
    /// within the recommended figure, column by column (VR-07), else M-14. On success the confirmation stamp is
    /// written and <c>redistribution_required</c> is cleared, in a <c>DISTRIBUTION_CONFIRM</c> scope.
    /// </summary>
    public sealed class ConfirmDistributionHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, TimeProvider? clock = null)
        : IRequestHandler<ConfirmDistributionCommand, Response>
    {
        public const string ActionCode = "DISTRIBUTION_CONFIRM";

        public async Task<Response> Handle(ConfirmDistributionCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var rec = await context.CeRecommendations
                .FirstOrDefaultAsync(r => r.RecommendationId == request.RecommendationId && r.CycleId == request.CycleId, ct);
            if (rec is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (rec.CeOfficeId != actor.OfficeId)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var (rbeTotal, obeTotal) = await context.Distributions.AsNoTracking()
                .Where(d => d.RecommendationId == request.RecommendationId)
                .GroupBy(_ => 1)
                .Select(g => new ValueTuple<decimal, decimal>(g.Sum(d => d.RbeAmount), g.Sum(d => d.ObeNextAmount)))
                .FirstOrDefaultAsync(ct);
            if (!DistributionRules.WithinCeiling(rec.RbeRecommended, rec.ObeNextRecommended, rbeTotal, obeTotal))
                return BudgetResponses.Fail(BudgetMessages.M14);

            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            rec.RedistributionRequired = false;
            rec.DistributionConfirmedAt = (clock ?? TimeProvider.System).GetUtcNow();
            rec.DistributionConfirmedBy = actor.EmployeeId;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = "Distribution confirmed" };
        }
    }
}
