using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.Delete
{
    public class DeleteBudgetEntryEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/budgets/{id:int}", async (int id, IMediator mediator) =>
            {
                var command = new DeleteBudgetEntryCommand(id);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    if (response.Message.Contains("not found"))
                        return Results.NotFound(response);
                    else
                        return Results.Conflict(response);
                }

                return Results.NoContent();
            })
            .WithName("DeleteBudgetEntry")
            .WithTags("BudgetEntries")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
