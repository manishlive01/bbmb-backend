using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Divisions.Delete
{
    public class DeleteDivisionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/divisions/{code:int}", async (int code, IMediator mediator) =>
            {
                var command = new DeleteDivisionCommand(code);
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
            .WithName("DeleteDivision")
            .WithTags("Divisions")
            .WithSummary("Delete a division")
            .WithDescription("Deletes a division")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
