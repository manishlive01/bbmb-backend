using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create
{
    public class CreateDivSectionManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/div-section-management", async (CreateDivSectionManagementDto dto, IMediator mediator) =>
            {
                var command = new CreateDivSectionManagementCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.Conflict(response);

                return Results.Created("/api/div-section-management", response);
            })
            .WithName("CreateDivSectionManagement")
            .WithTags("DivSectionManagement")
            .WithSummary("Create Division Section")
            .WithDescription("Creates a new Division Section record")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}