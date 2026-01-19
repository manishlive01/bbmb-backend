using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Delete
{
    public class DeleteDesignationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/designations/{code}", async (string code, IMediator mediator) =>
            {
                var command = new DeleteDesignationCommand(code);
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
            .WithName("DeleteDesignation")
            .WithTags("Designations")
            .WithSummary("Delete a designation")
            .WithDescription("Deletes a designation if it has no associated works")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
