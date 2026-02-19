using BBMB.BudgetService.Features.CEManagements.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.Update
{
    public class UpdateCEManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/ce-management/{id:int}", async (
                int id,
                UpdateCEManagementDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateCEManagementCommand(id, dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.NotFound(response);

                return Results.Ok(response);
            })
            .WithName("UpdateCEManagement")
            .WithTags("CEManagement")
            .WithSummary("Update CE Management record")
            .WithDescription("Updates an existing CE Management record by ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
