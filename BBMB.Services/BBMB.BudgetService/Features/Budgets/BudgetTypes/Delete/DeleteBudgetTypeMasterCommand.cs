using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.Delete
{
    public sealed record DeleteBudgetTypeMasterCommand(
        GetByIdRequestDto Request)
        : IRequest<Response>;
}
