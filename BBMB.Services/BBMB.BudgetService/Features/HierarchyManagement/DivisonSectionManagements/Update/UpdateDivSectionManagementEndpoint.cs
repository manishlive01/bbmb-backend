using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Update
{
    public class UpdateDivSectionManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/div-section-management", async (UpdateDivSectionManagementDto dto, IMediator mediator) =>
            {
                var command = new UpdateDivSectionManagementCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.Conflict(response);

                return Results.Ok(response);
            })
            .WithName("UpdateDivSectionManagement")
            .WithTags("DivSectionManagement")
            .WithSummary("Update Division Section")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}