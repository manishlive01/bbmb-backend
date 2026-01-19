using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Units.Delete
{
    public class DeleteUnitEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/units/{code}", async (string code, IMediator mediator) =>
            {
                var command = new DeleteUnitCommand(code);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.NotFound(response);
                }

                return Results.NoContent();
            })
            .WithName("DeleteUnit")
            .WithTags("Units")
            .WithSummary("Delete a unit")
            .WithDescription("Deletes a unit by its code")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
