using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Works.GetByDivision
{
    public class GetWorksByDivisionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/works/by-division/{divisionCode:int}", async (
                int divisionCode,
                IMediator mediator,
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10) =>
            {
                var query = new GetWorksByDivisionQuery(divisionCode, pageNumber, pageSize);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetWorksByDivision")
            .WithTags("Works")
            .Produces<PagedResponse<GetWorkListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
