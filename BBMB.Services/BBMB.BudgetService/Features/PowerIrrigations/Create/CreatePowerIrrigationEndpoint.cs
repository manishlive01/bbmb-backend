using BBMB.BudgetService.Features.PowerIrrigations.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.Create
{
    public class CreatePowerIrrigationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/power-irrigation", async (CreatePowerIrrigationDto dto, IMediator mediator) =>
            {
                var command = new CreatePowerIrrigationCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/power-irrigation/{dto.IrrPwrCode}", response);
            })
            .WithName("CreatePowerIrrigation")
            .WithTags("PowerIrrigation")
            .WithSummary("Create a new power/irrigation code")
            .WithDescription("Creates a new power/irrigation code entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
