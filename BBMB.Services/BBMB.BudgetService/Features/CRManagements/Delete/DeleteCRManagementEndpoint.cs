using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.Delete
{
    public class DeleteCRManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/cr-management/{crCode}", async (
                int crCode,
                IMediator mediator) =>
            {
                var command = new DeleteCRManagementCommand(crCode);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.NotFound(response);

                return Results.Ok(response);
            })
            .WithName("DeleteCRManagement")
            .WithTags("CRManagement")
            .WithSummary("Delete CR Management record")
            .WithDescription("Deletes CR Management record by ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
