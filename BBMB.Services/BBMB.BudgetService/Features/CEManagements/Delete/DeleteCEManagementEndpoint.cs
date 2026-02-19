using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.Delete
{
    public class DeleteCEManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/ce-management/{id:int}", async (
                int id,
                IMediator mediator) =>
            {
                var command = new DeleteCEManagementCommand(id);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.NotFound(response);

                return Results.Ok(response);
            })
            .WithName("DeleteCEManagement")
            .WithTags("CEManagement")
            .WithSummary("Delete CE Management record")
            .WithDescription("Deletes a CE Management record by ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
