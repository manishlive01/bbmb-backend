using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Estimates.GetByFinancialYear
{
    public class GetEstimatesByFinancialYearEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/estimates/by-financial-year/{finYearEnd:int}", async (
                int finYearEnd,
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var query = new GetEstimatesByFinancialYearQuery(finYearEnd, pageNumber, pageSize);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetEstimatesByFinancialYear")
            .WithTags("Estimates")
            .Produces<PagedResponse<GetEstimateListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
