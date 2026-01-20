using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetList
{
    public class GetSubMinorHeadListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/sub-minor-heads", async (
                string? subMinorAccCode,
                string? majorAccCode,
                string? subMajorAccCode,
                string? minorAccCode,
                string? subMinorAccDesc,
                string? userCode,
                DateTime? modificationDateFrom,
                DateTime? modificationDateTo,
                string? sortBy,
                bool sortDescending,
                int pageNumber,
                int pageSize,
                IMediator mediator) =>
            {
                var query = new GetSubMinorHeadListQuery(
                    subMinorAccCode,
                    majorAccCode,
                    subMajorAccCode,
                    minorAccCode,
                    subMinorAccDesc,
                    userCode,
                    modificationDateFrom,
                    modificationDateTo,
                    sortBy,
                    sortDescending,
                    pageNumber == 0 ? 1 : pageNumber,
                    pageSize == 0 ? 10 : pageSize
                );

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetSubMinorHeadList")
            .WithTags("SubMinorHeads")
            .Produces<PagedResponse<GetSubMinorHeadListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
