using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Estimates.GetList
{
    public class GetEstimateListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/estimates", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] int? workCode = null,
                [FromQuery] int? divisionCode = null,
                [FromQuery] int? finYearEnd = null) =>
            {
                var query = new GetEstimateListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    workCode, divisionCode, finYearEnd);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetEstimateList")
            .WithTags("Estimates")
            .Produces<PagedResponse<GetEstimateListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
