using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Budgets.GetByDivision
{
    public class GetBudgetEntriesByDivisionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budgets/by-division/{divisionCode:int}", async (
                int divisionCode,
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                IMediator mediator) =>
            {
                var query = new GetBudgetEntriesByDivisionQuery(divisionCode, pageNumber, pageSize);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetBudgetEntriesByDivision")
            .WithTags("BudgetEntries")
            .Produces<PagedResponse<GetBudgetEntryListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
