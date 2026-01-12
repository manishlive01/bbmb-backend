using BBMB.BudgetService.Features.Budgets.BudgetTypes.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.Create
{
    public sealed record CreateBudgetTypeMasterCommand(CreateBudgetTypeMasterDto dto) : IRequest<Response>;
}
