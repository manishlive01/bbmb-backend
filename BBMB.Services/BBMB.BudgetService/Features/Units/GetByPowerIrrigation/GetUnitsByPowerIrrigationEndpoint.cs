using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Units.GetByPowerIrrigation
{
    public class GetUnitsByPowerIrrigationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/units/by-power-irrigation/{code}", async (string code, IMediator mediator) =>
            {
                var query = new GetUnitsByPowerIrrigationQuery(code);
                var response = await mediator.Send(query);

                return Results.Ok(response);
            })
            .WithName("GetUnitsByPowerIrrigation")
            .WithTags("Units")
            .WithSummary("Get units by power/irrigation code")
            .WithDescription("Retrieves all units with the specified power/irrigation classification")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
