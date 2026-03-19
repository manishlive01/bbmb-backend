using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.Create
{
    public class CreateDivisionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/divisions", async (CreateDivisionDto dto, IMediator mediator) =>
            {
                var command = new CreateDivisionCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/divisions/{dto.DivisionCode}", response);
            })
            .WithName("CreateDivision")
            .WithTags("Divisions")
            .WithSummary("Create a new division")
            .WithDescription("Creates a new division entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
