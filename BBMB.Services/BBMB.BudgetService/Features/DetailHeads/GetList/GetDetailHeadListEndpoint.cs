using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.DetailHeads.GetList
{
    public class GetDetailHeadListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/detail-heads", async (
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                [FromQuery] string? sortBy,
                [FromQuery] bool sortDescending,
                [FromQuery] string? detailAccCode,
                [FromQuery] string? majorAccCode,
                [FromQuery] string? subMajorAccCode,
                [FromQuery] string? minorAccCode,
                [FromQuery] string? subMinorAccCode,
                [FromQuery] string? detailAccDesc,
                [FromQuery] string? userCode,
                [FromQuery] DateTime? modificationDateFrom,
                [FromQuery] DateTime? modificationDateTo,
                ISender sender) =>
            {
                var query = new GetDetailHeadListQuery(
                    pageNumber > 0 ? pageNumber : 1,
                    pageSize > 0 ? pageSize : 10,
                    sortBy,
                    sortDescending,
                    detailAccCode,
                    majorAccCode,
                    subMajorAccCode,
                    minorAccCode,
                    subMinorAccCode,
                    detailAccDesc,
                    userCode,
                    modificationDateFrom,
                    modificationDateTo
                );

                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetDetailHeadList")
            .WithTags("DetailHeads")
            .Produces(200);
        }
    }
}
