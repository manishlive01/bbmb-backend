using BBMB.BudgetService.Features.BudgetProcess.Reports.GetDashboard.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetDashboard
{
    public sealed class GetDashboardEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-dashboard",
                    async (long cycleId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetDashboardQuery(cycleId), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetBudgetDashboard")
                .WithTags("BudgetReports")
                .Produces<Response<DashboardDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
