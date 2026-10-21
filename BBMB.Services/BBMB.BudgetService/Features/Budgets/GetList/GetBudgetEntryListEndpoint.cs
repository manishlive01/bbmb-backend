using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Budgets.GetList
{
    public class GetBudgetEntryListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budgets", async (
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                IMediator mediator,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] int? divisionCode = null,
                [FromQuery] int? calendarYear = null,
                [FromQuery] BudgetType? budgetType = null,
                [FromQuery] int? workId = null) =>
            {
                var query = new GetBudgetEntryListQuery(
                    pageNumber,
                    pageSize,
                    sortBy,
                    sortDescending,
                    divisionCode,
                    calendarYear,
                    budgetType,
                    workId);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetBudgetEntryList")
            .WithTags("BudgetEntries")
            .Produces<PagedResponse<GetBudgetEntryListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
