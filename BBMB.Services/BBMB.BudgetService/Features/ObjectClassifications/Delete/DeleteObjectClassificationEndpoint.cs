using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.Delete
{
    public class DeleteObjectClassificationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/object-classifications/{code}", async (string code, IMediator mediator) =>
            {
                var command = new DeleteObjectClassificationCommand(code);
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
            .WithName("DeleteObjectClassification")
            .WithTags("ObjectClassifications")
            .WithSummary("Delete an object classification")
            .WithDescription("Deletes an object classification if it has no associated records")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
