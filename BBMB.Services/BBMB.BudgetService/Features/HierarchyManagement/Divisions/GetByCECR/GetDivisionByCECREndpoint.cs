using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR
{
    public class GetDivisionByCECREndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/divisions/by-ce-cr/{ceId:int}/{crId:int}",
async ([FromRoute] int ceId,
       [FromRoute] int crId,
       IMediator mediator) =>
{
    var query = new GetDivisionByCECRQuery(ceId, crId);

    var result = await mediator.Send(query);

    return Results.Ok(result);
})
.WithName("GetDivisionByCECR")
.WithTags("Divisions")
.WithSummary("Get divisions by CEId and CRId")
.WithDescription("Retrieves divisions filtered by CEId and CRId")
.Produces(StatusCodes.Status200OK);
        }
    }
}