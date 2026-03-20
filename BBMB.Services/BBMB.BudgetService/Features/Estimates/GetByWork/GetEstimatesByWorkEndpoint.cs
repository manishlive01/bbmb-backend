using BBMB.BudgetService.Features.Estimates.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Estimates.GetByWork
{
    public class GetEstimatesByWorkEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/estimates/by-work/{workCode}", async (
                int workCode,
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var query = new GetEstimatesByWorkQuery(workCode, pageNumber, pageSize);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetEstimatesByWork")
            .WithTags("Estimates")
            .Produces<PagedResponse<GetEstimateListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
