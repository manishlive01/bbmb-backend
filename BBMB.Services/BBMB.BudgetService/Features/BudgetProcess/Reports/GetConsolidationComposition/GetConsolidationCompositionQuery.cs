using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidationComposition.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidationComposition
{
    /// <summary>
    /// Design §8.8 (<c>GET /api/budget-cycles/{cycleId}/consolidation/composition?key=</c>, BUD-178): the demand
    /// lines that make up a consolidation cell. <paramref name="Key"/> is <c>group:value</c>
    /// (e.g. <c>head:1234-00-...</c>, <c>ce:5</c>, <c>division:12</c>, <c>wing:PW</c>).
    /// </summary>
    public sealed record GetConsolidationCompositionQuery(long CycleId, string Key, string? Category)
        : IRequest<Response<List<CompositionLineDto>>>;
}
