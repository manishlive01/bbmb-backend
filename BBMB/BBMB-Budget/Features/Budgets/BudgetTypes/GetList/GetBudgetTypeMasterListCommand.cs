using BBMB_Budget.Features.Budgets.BudgetTypes.GetList.Dtos;
using MediatR;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.GetList
{
    public sealed record GetBudgetTypeMasterListCommand() : IRequest<List<GetBudgetTypeMasterListResponseDto>>;
}
