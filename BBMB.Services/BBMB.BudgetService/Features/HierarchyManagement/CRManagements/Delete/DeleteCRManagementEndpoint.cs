using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Delete
{
    public class DeleteCRManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/cr-management/{crId}", async (
                int crId,
                IMediator mediator) =>
            {
                var command = new DeleteCRManagementCommand(crId);
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
