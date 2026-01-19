using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Works.GetList
{
    public class GetWorkListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/works", async (
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] string? workCode = null,
                [FromQuery] string? workDesc = null,
                [FromQuery] int? divisionCode = null,
                [FromQuery] string? status = null,
                [FromQuery] string? nature = null) =>
            {
                var query = new GetWorkListQuery(
                    pageNumber, pageSize, sortBy, sortDescending,
                    workCode, workDesc, divisionCode, status, nature);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetWorkList")
            .WithTags("Works")
            .Produces<PagedResponse<GetWorkListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
