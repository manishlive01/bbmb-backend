using BBMB.BudgetService.Features.Budgets.BudgetTypes.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs; 
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.Update
{
    public sealed record UpdateBudgetTypeMasterCommand(UpdateBudgetTypeMasterDto dto) : IRequest<Response>;

}
