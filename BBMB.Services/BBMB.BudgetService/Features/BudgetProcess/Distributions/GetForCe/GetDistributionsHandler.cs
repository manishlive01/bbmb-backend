using BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe
{
    /// <summary>
    /// Design §8.5, §10 ("view own share"). Order: actor (M-20) → the CE's recommendations of the cycle and
    /// category from V-05, each with its shares (T-39). The CE office (and offices that see it in their visible
    /// set) see every share; an office below sees only the share distributed to an office in its own visible set
    /// (BUD-160). No row for the CE returns an empty list, not an error.
    /// </summary>
    public sealed class GetDistributionsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetDistributionsQuery, Response<List<DistributionHeadDto>>>
    {
        public async Task<Response<List<DistributionHeadDto>>> Handle(GetDistributionsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<DistributionHeadDto>>(BudgetMessages.M20);
            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
            var seesAllShares = visible.Contains(request.CeOfficeId);

            var balances = await context.DistributionBalances.AsNoTracking()
                .Where(b => b.CycleId == request.CycleId && b.Category == request.Category && b.CeOfficeId == request.CeOfficeId)
                .ToListAsync(ct);
            if (balances.Count == 0)
                return new Response<List<DistributionHeadDto>> { IsSucced = true, Message = "Distributions", Data = [] };

            var recIds = balances.Select(b => b.RecommendationId).ToList();
            var shares = await context.Distributions.AsNoTracking()
                .Where(d => recIds.Contains(d.RecommendationId))
                .Select(d => new { d.RecommendationId, d.OfficeId, d.RbeAmount, d.ObeNextAmount })
                .ToListAsync(ct);
            var codeIds = balances.Select(b => b.AccountCodeId).Distinct().ToList();
            var codes = await context.HeadOfAccounts.AsNoTracking()
                .Where(h => codeIds.Contains(h.AccountCodeId))
                .Select(h => new { h.AccountCodeId, h.CompleteCode }).ToDictionaryAsync(h => h.AccountCodeId, h => h.CompleteCode, ct);

            var data = balances.Select(b =>
            {
                var mine = shares.Where(s => s.RecommendationId == b.RecommendationId)
                    .Where(s => seesAllShares || visible.Contains(s.OfficeId))
                    .Select(s => new DistributionShareDto(s.OfficeId, s.RbeAmount, s.ObeNextAmount))
                    .OrderBy(s => s.OfficeId)
                    .ToList();
                return new DistributionHeadDto(
                    b.RecommendationId, b.AccountCodeId, codes.GetValueOrDefault(b.AccountCodeId),
                    b.RbeRecommended, b.ObeNextRecommended, b.RbeDistributed, b.ObeDistributed,
                    b.RbeBalanceRetained, b.ObeBalanceRetained, b.RedistributionRequired, mine);
            }).OrderBy(h => h.AccountCodeId).ToList();

            return new Response<List<DistributionHeadDto>> { IsSucced = true, Message = "Distributions", Data = data };
        }
    }
}
