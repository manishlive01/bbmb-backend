using BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList
{
    public sealed record GetBudgetTypeMasterListCommand() : IRequest<List<GetBudgetTypeMasterListResponseDto>>;
}
