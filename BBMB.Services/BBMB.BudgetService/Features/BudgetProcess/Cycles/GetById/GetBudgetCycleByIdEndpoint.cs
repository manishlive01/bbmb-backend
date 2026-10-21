using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetById
{
    public sealed class GetBudgetCycleByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles/{id:long}", async (long id, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetBudgetCycleByIdQuery(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetBudgetCycleById")
                .WithTags("BudgetCycles")
                .Produces<Response<BudgetCycleResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
