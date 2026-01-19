using BBMB.BudgetService.Features.PowerIrrigations.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.Update
{
    public class UpdatePowerIrrigationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/power-irrigation/{code}", async (string code, UpdatePowerIrrigationDto dto, IMediator mediator) =>
            {
                var command = new UpdatePowerIrrigationCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdatePowerIrrigation")
            .WithTags("PowerIrrigation")
            .WithSummary("Update a power/irrigation code")
            .WithDescription("Updates an existing power/irrigation code's description and user code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
