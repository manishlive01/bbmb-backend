using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.GetByWork
{
    public sealed record GetBudgetEntriesByWorkQuery(
        string WorkCode,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetBudgetEntryListResponseDto>>;
}
