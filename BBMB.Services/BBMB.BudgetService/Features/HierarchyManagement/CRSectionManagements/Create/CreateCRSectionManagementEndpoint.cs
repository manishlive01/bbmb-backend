using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create
{
    public class CreateCRSectionManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/cr-section-management", async (CreateCRSectionManagementDto dto, IMediator mediator) =>
            {
                var command = new CreateCRSectionManagementCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/cr-section-management", response);
            })
            .WithName("CreateCRSectionManagement")
            .WithTags("CRSectionManagement")
            .WithSummary("Create a new CR Section")
            .WithDescription("Creates a new CR Section record")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}