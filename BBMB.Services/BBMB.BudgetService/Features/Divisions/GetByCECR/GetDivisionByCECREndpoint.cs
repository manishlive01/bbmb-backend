using BBMB.BudgetService.Features.Divisions.GetByCECR;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Divisions.GetByCECR
{
    public class GetDivisionByCECREndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/divisions/by-ce-cr", async (
                IMediator mediator,
                [FromQuery] int ceCode,
                [FromQuery] int crCode) =>
            {
                var query = new GetDivisionByCECRQuery(ceCode, crCode);

                var result = await mediator.Send(query);

                return Results.Ok(result);
            })
            .WithName("GetDivisionByCECR")
            .WithTags("Divisions")
            .WithSummary("Get divisions by CECode and CRCode")
            .WithDescription("Retrieves divisions filtered by CECode and CRCode")
            .Produces(StatusCodes.Status200OK);
        }
    }
}