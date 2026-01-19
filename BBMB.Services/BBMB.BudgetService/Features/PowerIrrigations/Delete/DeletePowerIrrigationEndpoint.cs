using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.Delete
{
    public class DeletePowerIrrigationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/power-irrigation/{code}", async (string code, IMediator mediator) =>
            {
                var command = new DeletePowerIrrigationCommand(code);
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
            .WithName("DeletePowerIrrigation")
            .WithTags("PowerIrrigation")
            .WithSummary("Delete a power/irrigation code")
            .WithDescription("Deletes a power/irrigation code")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
