using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR
{
    public class GetDivisionByCECREndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/divisions/by-ce-cr", async (
                IMediator mediator,
                [FromQuery] int ceId,
                [FromQuery] int crId) =>
            {
                var query = new GetDivisionByCECRQuery(ceId, crId);

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