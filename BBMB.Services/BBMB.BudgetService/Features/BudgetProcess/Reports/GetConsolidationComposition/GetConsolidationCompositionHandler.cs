using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reports.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidationComposition.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidationComposition
{
    /// <summary>
    /// Design §8.8 (BUD-178). Order: actor (M-20) → the key parses as <c>group:value</c>, else M-22. The demand
    /// lines of <c>v_report_demand</c> for the cycle that match the key are returned (the lines whose grouped
    /// totals sum to the consolidation cell). Finance and Secretariat see all; another office is scoped to its
    /// visible offices.
    /// </summary>
    public sealed class GetConsolidationCompositionHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetConsolidationCompositionQuery, Response<List<CompositionLineDto>>>
    {
        public async Task<Response<List<CompositionLineDto>>> Handle(GetConsolidationCompositionQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<CompositionLineDto>>(BudgetMessages.M20);

            var parts = (request.Key ?? string.Empty).Split(':', 2);
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
                return BudgetResponses.Fail<List<CompositionLineDto>>(BudgetMessages.M22);
            var (group, value) = (parts[0].Trim().ToLowerInvariant(), parts[1].Trim());

            IQueryable<ReportDemandRow> q = context.ReportDemand.AsNoTracking().Where(r => r.CycleId == request.CycleId);
            if (!string.IsNullOrWhiteSpace(request.Category))
                q = q.Where(r => r.Category == request.Category);
            q = group switch
            {
                "wing" => q.Where(r => r.WingCode == value),
                "ce" => int.TryParse(value, out var ce) ? q.Where(r => r.CeId == ce) : q.Where(_ => false),
                "circle" => int.TryParse(value, out var cr) ? q.Where(r => r.CircleId == cr) : q.Where(_ => false),
                "division" => int.TryParse(value, out var dv) ? q.Where(r => r.DivisionId == dv) : q.Where(_ => false),
                "head" => q.Where(r => r.CompleteCode == value),
                _ => null!,
            };
            if (q is null)
                return BudgetResponses.Fail<List<CompositionLineDto>>(BudgetMessages.M22);

            if (actor.OfficeLevel is not (OfficeLevel.Finance or OfficeLevel.Secretariat))
            {
                var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
                q = q.Where(r => visible.Contains(r.OriginOfficeId));
            }

            var data = await q
                .OrderBy(r => r.OriginOfficeId).ThenBy(r => r.DemandLineId)
                .Select(r => new CompositionLineDto(
                    r.DemandLineId, r.ProposalId, r.OriginOfficeId, r.LineType, r.WorkId, r.AccountCodeId,
                    r.CompleteCode, r.DemandedRbe ?? 0m, r.DemandedObeNext ?? 0m))
                .ToListAsync(ct);

            return new Response<List<CompositionLineDto>> { IsSucced = true, Message = "Composition", Data = data };
        }
    }
}
