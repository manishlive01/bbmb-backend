using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList
{
    public class GetSubMajorHeadListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/accounting-heads/sub-major", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? subMajorAccCode = null,
                [FromQuery] string? majorAccCode = null,
                [FromQuery] string? subMajorAccDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetSubMajorHeadListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    subMajorAccCode, majorAccCode, subMajorAccDesc, userCode,
                    modificationDateFrom, modificationDateTo);
                    
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetSubMajorHeadList")
            .WithTags("SubMajorHeads")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
