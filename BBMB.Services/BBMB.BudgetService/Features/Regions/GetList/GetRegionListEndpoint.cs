using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Regions.GetList
{
    public class GetRegionListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/regions", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? regionCode = null,
                [FromQuery] string? regionDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetRegionListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    regionCode, regionDesc, userCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetRegionList")
            .WithTags("Regions")
            .WithSummary("Get a paginated list of regions")
            .WithDescription("Retrieves regions with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
