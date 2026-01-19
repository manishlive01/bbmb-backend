using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.GetByCalendarYear
{
    public sealed record GetBudgetEntriesByCalendarYearQuery(
        int CalendarYear,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetBudgetEntryListResponseDto>>;
}
