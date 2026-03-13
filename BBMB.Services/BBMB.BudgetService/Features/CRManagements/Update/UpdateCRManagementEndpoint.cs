using BBMB.BudgetService.Features.CRManagements.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.Update
{
    public class UpdateCRManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/cr-management/{crCode}", async (
                int crCode,
                UpdateCRManagementDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateCRManagementCommand(crCode, dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.NotFound(response);

                return Results.Ok(response);
            })
            .WithName("UpdateCRManagement")
            .WithTags("CRManagement")
            .WithSummary("Update CR Management")
            .WithDescription("Updates an existing CR Management record by ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
