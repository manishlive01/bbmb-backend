using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Units.GetList
{
    public class GetUnitListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/units", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? unitCode = null,
                [FromQuery] string? unitDesc = null,
                [FromQuery] string? projCode = null,
                [FromQuery] string? irrPwrCode = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetUnitListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    unitCode, unitDesc, projCode, irrPwrCode, userCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetUnitList")
            .WithTags("Units")
            .WithSummary("Get a paginated list of units")
            .WithDescription("Retrieves units with filtering, sorting, pagination, and joined project/power-irrigation data")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
