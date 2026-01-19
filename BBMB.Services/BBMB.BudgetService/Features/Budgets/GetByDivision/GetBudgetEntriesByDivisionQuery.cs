using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.GetByDivision
{
    public sealed record GetBudgetEntriesByDivisionQuery(
        int DivisionCode,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetBudgetEntryListResponseDto>>;
}
