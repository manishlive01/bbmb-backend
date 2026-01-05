using BBMB_Budget.Features.Budgets.BudgetTypes.Create.Dtos;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.Create
{
    public sealed record CreateBudgetTypeMasterCommand(CreateBudgetTypeMasterDto dto) : IRequest<Response>;
}
