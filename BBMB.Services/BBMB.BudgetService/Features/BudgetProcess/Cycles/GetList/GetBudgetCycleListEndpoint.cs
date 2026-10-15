using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetList
{
    public sealed class GetBudgetCycleListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles", async (
                    IMediator mediator, CancellationToken ct,
                    [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null) =>
                    Results.Ok(await mediator.Send(new GetBudgetCycleListQuery(pageNumber, pageSize, status), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetBudgetCycleList")
                .WithTags("BudgetCycles")
                .Produces<PagedResponse<BudgetCycleResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
