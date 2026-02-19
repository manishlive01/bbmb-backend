using BBMB.BudgetService.Features.CEManagements.Create;
using BBMB.BudgetService.Features.CEManagements.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.Create
{
    public class CreateCEManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/ce-management", async (CreateCEManagementDto dto, IMediator mediator) =>
            {
                var command = new CreateCEManagementCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/ce-management", response);
            })
            .WithName("CreateCEManagement")
            .WithTags("CEManagement")
            .WithSummary("Create a new CE record")
            .WithDescription("Creates a new Chief Engineer record")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
