using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetCurrent
{
    public sealed class GetCurrentBudgetCycleEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles/current", async (IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetCurrentBudgetCycleQuery(), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetCurrentBudgetCycle")
                .WithTags("BudgetCycles")
                .Produces<Response<BudgetCycleResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
