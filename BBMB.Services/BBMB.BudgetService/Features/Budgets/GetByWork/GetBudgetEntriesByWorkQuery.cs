using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.GetByWork
{
    // P4 (design §4.6): entries of a work are addressed by the work's stable id, not the old int code.
    public sealed record GetBudgetEntriesByWorkQuery(
        int WorkId,
        int PageNumber,
        int PageSize
    ) : IRequest<PagedResponse<GetBudgetEntryListResponseDto>>;
}
