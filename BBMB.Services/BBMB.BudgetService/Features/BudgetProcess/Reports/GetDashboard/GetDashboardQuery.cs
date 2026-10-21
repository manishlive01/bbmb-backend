using BBMB.BudgetService.Features.BudgetProcess.Reports.GetDashboard.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetDashboard
{
    /// <summary>Design §8.8 (<c>GET /api/budget-dashboard?cycleId=</c>, F-25, BUD-219).</summary>
    public sealed record GetDashboardQuery(long CycleId) : IRequest<Response<DashboardDto>>;
}
