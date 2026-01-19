using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PowerIrrigations.GetById
{
    public class GetPowerIrrigationByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/power-irrigation/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetPowerIrrigationByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new
                    {
                        message = "Power/Irrigation code retrieved successfully",
                        data = result
                    });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        error = "power_irrigation_not_found",
                        message = ex.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            })
            .WithName("GetPowerIrrigationById")
            .WithTags("PowerIrrigation")
            .WithSummary("Get a power/irrigation code by code")
            .WithDescription("Retrieves a specific power/irrigation code by its code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
