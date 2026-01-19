using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Divisions.GetList
{
    public class GetDivisionListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/divisions", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] int? divisionCode = null,
                [FromQuery] string? divisionDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] string? regionCode = null,
                [FromQuery] string? poolCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetDivisionListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    divisionCode, divisionDesc, userCode, regionCode, poolCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetDivisionList")
            .WithTags("Divisions")
            .WithSummary("Get a paginated list of divisions")
            .WithDescription("Retrieves divisions with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
