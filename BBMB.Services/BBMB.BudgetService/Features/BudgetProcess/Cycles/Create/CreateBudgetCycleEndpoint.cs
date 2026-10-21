using BBMB.BudgetService.Features.BudgetProcess.Cycles.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.Create
{
    public sealed class CreateBudgetCycleEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-cycles", async (CreateBudgetCycleDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.From(await mediator.Send(new CreateBudgetCycleCommand(dto), ct),
                        r => Results.Created($"/api/budget-cycles/{r.Data}", r)))
                .RequireAuthorization("BudgetRight:Cycle.Manage")
                .WithName("CreateBudgetCycle")
                .WithTags("BudgetCycles")
                .Produces<Response<long>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
