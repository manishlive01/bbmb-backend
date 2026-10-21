using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Features.BudgetProcess.Recommendations.GetRows.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.GetRows
{
    /// <summary>
    /// Design §8.3 GetRows: the (CE, head) rows of V-03 joined with T-37, plus recommendations that have no demand
    /// row, ordered by CE and code. Scope: the rows of CE offices visible to the actor (§4.4) — Finance and the
    /// Secretariat see all, a CE its own, a Circle or Division none. Cycle absent → NOT_FOUND.
    /// </summary>
    public sealed class GetRecommendationRowsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetRecommendationRowsQuery, Response<List<RecommendationRowDto>>>
    {
        public async Task<Response<List<RecommendationRowDto>>> Handle(GetRecommendationRowsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<RecommendationRowDto>>(BudgetMessages.M20);
            if (!await context.BudgetCycles.AsNoTracking().AnyAsync(c => c.CycleId == request.CycleId, ct))
                return BudgetResponses.Fail<List<RecommendationRowDto>>(BudgetMessages.NotFound);
            var visible = (await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).ToList();
            var demand = await context.DemandByCe.AsNoTracking()
                .Where(r => r.CycleId == request.CycleId && r.Category == request.Category && visible.Contains(r.CeOfficeId))
                .ToListAsync(ct);
            var recs = await context.CeRecommendations.AsNoTracking()
                .Where(r => r.CycleId == request.CycleId && r.Category == request.Category && visible.Contains(r.CeOfficeId))
                .ToListAsync(ct);
            var keys = demand.Select(d => (d.CeOfficeId, d.AccountCodeId))
                .Union(recs.Select(r => (r.CeOfficeId, r.AccountCodeId)))
                .OrderBy(k => k.CeOfficeId).ThenBy(k => k.AccountCodeId).ToList();
            var codeIds = keys.Select(k => k.AccountCodeId).Distinct().ToList();
            var heads = await context.HeadOfAccounts.AsNoTracking().Where(h => codeIds.Contains(h.AccountCodeId))
                .Select(h => new { h.AccountCodeId, h.CompleteCode, h.HeadDesc }).ToDictionaryAsync(h => h.AccountCodeId, ct);
            var names = await BudgetOfficeNames.ResolveAsync(context, keys.Select(k => k.CeOfficeId), ct);
            var rows = keys.Select(k =>
            {
                var d = demand.FirstOrDefault(x => x.CeOfficeId == k.CeOfficeId && x.AccountCodeId == k.AccountCodeId);
                var r = recs.FirstOrDefault(x => x.CeOfficeId == k.CeOfficeId && x.AccountCodeId == k.AccountCodeId);
                var h = heads.GetValueOrDefault(k.AccountCodeId);
                return new RecommendationRowDto
                {
                    CeOfficeId = k.CeOfficeId, CeOfficeName = names.GetValueOrDefault(k.CeOfficeId), AccountCodeId = k.AccountCodeId,
                    CompleteCode = h?.CompleteCode, HeadDesc = h?.HeadDesc,
                    RbeDemanded = d?.RbeTotal, ObeNextDemanded = d?.ObeNextTotal, LineCount = d?.LineCount ?? 0, OfficeCount = d?.OfficeCount ?? 0,
                    RecommendationId = r?.RecommendationId, RbeRecommended = r?.RbeRecommended, ObeNextRecommended = r?.ObeNextRecommended,
                    ConcurrenceRef = r?.ConcurrenceRef,
                };
            }).ToList();
            return new Response<List<RecommendationRowDto>> { IsSucced = true, Message = "Recommendation rows retrieved successfully", Data = rows };
        }
    }
}
