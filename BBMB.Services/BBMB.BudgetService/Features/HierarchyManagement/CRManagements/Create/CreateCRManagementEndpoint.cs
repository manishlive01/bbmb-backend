using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Create
{
    public class CreateCRManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/cr-management", async (
                CreateCRManagementDto dto,
                IMediator mediator) =>
            {
                var command = new CreateCRManagementCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.BadRequest(response);

                return Results.Created("/api/cr-management", response);
            })
            .WithName("CreateCRManagement")
            .WithTags("CRManagement")
            .WithSummary("Create CR Management")
            .WithDescription("Creates a new Circle Management record")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
