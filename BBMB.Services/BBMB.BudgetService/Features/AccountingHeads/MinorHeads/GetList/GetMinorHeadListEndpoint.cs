using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList
{
    public class GetMinorHeadListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/minor-heads", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? minorAccCode = null,
                [FromQuery] string? majorAccCode = null,
                [FromQuery] string? subMajorAccCode = null,
                [FromQuery] string? minorAccDesc = null,
                [FromQuery] string? userCode = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetMinorHeadListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    minorAccCode, majorAccCode, subMajorAccCode, minorAccDesc, userCode,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetMinorHeadList")
            .WithTags("MinorHeads")
            .Produces<PagedResponse<GetMinorHeadListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
