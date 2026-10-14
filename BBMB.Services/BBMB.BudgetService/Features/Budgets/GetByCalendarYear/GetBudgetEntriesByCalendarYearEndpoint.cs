using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Budgets.GetByCalendarYear
{
    public class GetBudgetEntriesByCalendarYearEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budgets/by-calendar-year/{calendarYear:int}", async (
                int calendarYear,
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                IMediator mediator) =>
            {
                var query = new GetBudgetEntriesByCalendarYearQuery(calendarYear, pageNumber, pageSize);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetBudgetEntriesByCalendarYear")
            .WithTags("BudgetEntries")
            .Produces<PagedResponse<GetBudgetEntryListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
