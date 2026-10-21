using BBMB.BudgetService.Features.HeadOfAccounts.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HeadOfAccounts.GetList
{
    public class GetHeadOfAccountListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/head-of-accountLists", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? majorAccCode = null,
                [FromQuery] string? subMajorAccCode = null,
                [FromQuery] string? minorAccCode = null,
                [FromQuery] string? subMinorAccCode = null,
                [FromQuery] string? detailAccCode = null,
                [FromQuery] string? objectAccCode = null,
                [FromQuery] string? headDesc = null,
                [FromQuery] DateTime? modificationDateFrom = null,
                [FromQuery] DateTime? modificationDateTo = null) =>
            {
                var query = new GetHeadOfAccountListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    majorAccCode, subMajorAccCode, minorAccCode, subMinorAccCode,
                    detailAccCode, objectAccCode, headDesc,
                    modificationDateFrom, modificationDateTo);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetHeadOfAccountList")
            .WithTags("HeadOfAccounts")
            .Produces<PagedResponse<GetHeadOfAccountListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
