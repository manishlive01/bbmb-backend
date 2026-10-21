using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetList
{
    public sealed record GetBudgetCycleListQuery(int PageNumber, int PageSize, string? Status)
        : IRequest<PagedResponse<BudgetCycleResponseDto>>;
}
