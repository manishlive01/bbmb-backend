using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Units.GetByProject
{
    public class GetUnitsByProjectEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/units/by-project/{projCode}", async (string projCode, IMediator mediator) =>
            {
                var query = new GetUnitsByProjectQuery(projCode);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetUnitsByProject")
            .WithTags("Units")
            .WithSummary("Get units by project code")
            .WithDescription("Retrieves all units belonging to a specific project with joined data")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
