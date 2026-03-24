using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update
{
    public class UpdateCRSectionManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/cr-section-management", async (UpdateCRSectionManagementDto dto, IMediator mediator) =>
            {
                var command = new UpdateCRSectionManagementCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.Conflict(response);

                return Results.Ok(response);
            })
            .WithName("UpdateCRSectionManagement")
            .WithTags("CRSectionManagement")
            .WithSummary("Update CR Section")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}