using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Designations.GetList
{
    public class GetDesignationListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/designations", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? designationCode = null,
                [FromQuery] string? designationDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetDesignationListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    designationCode, designationDesc, userCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetDesignationList")
            .WithTags("Designations")
            .WithSummary("Get a paginated list of designations")
            .WithDescription("Retrieves designations with filtering, sorting, and pagination")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
