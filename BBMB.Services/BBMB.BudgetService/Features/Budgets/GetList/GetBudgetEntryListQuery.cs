using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.GetList
{
    public sealed record GetBudgetEntryListQuery(
        int PageNumber,
        int PageSize,
        string? SortBy,
        bool SortDescending,
        int? DivisionCode,
        int? CalendarYear,
        BudgetType? BudgetType,
        // P4 (design §4.6, §9.3 Budgets P4): the list filters by work id, not the old int work code.
        int? WorkId
    ) : IRequest<PagedResponse<GetBudgetEntryListResponseDto>>;
}
