using BBMB.BudgetService.Features.Units.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Units.Create
{
    public class CreateUnitEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/units", async (CreateUnitDto dto, IMediator mediator) =>
            {
                var command = new CreateUnitCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    // Check if it's a validation error (400) or conflict (409)
                    if (response.Message.Contains("already exists"))
                    {
                        return Results.Conflict(response);
                    }
                    return Results.BadRequest(response);
                }

                return Results.Created($"/api/units/{dto.UnitCode}", response);
            })
            .WithName("CreateUnit")
            .WithTags("Units")
            .WithSummary("Create a new unit")
            .WithDescription("Creates a new unit entry with project and power/irrigation associations")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
