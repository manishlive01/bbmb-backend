using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.Delete
{
    public class DeleteRegionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/regions/{code}", async (string code, IMediator mediator) =>
            {
                var command = new DeleteRegionCommand(code);
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
            .WithName("DeleteRegion")
            .WithTags("Regions")
            .WithSummary("Delete a region")
            .WithDescription("Deletes a region")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
