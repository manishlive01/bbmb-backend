using BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetConsolidation
{
    /// <summary>
    /// Design §8.8 (<c>GET /api/budget-cycles/{cycleId}/consolidation?category=&amp;wing=&amp;groupBy=</c>,
    /// F-18, BUD-311). Totals of the demanded figures grouped by the requested key, computed in SQL on the
    /// unrounded values (BUD-177, BUD-266). Allocation/freeze figures are 'not yet available' until P4/P5.
    /// </summary>
    public sealed record GetConsolidationQuery(long CycleId, string? Category, string? Wing, string? GroupBy)
        : IRequest<Response<ConsolidationResultDto>>;
}
