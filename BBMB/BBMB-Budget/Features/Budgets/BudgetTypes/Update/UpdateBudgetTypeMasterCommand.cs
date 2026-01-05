using BBMB_Budget.Features.Budgets.BudgetTypes.Update.Dtos;
using BBMB_Budget.Features.Geographical.States.Update.Dtos;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.Update
{
    public sealed record UpdateBudgetTypeMasterCommand(UpdateBudgetTypeMasterDto dto) : IRequest<Response>;

}
