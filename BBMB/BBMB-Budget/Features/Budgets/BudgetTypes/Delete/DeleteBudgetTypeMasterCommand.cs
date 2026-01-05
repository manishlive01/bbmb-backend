using BBMB_Budget.Shared.RequestDTOs;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.Delete
{
    public sealed record DeleteBudgetTypeMasterCommand(
        GetByIdRequestDto Request)
        : IRequest<Response>;
}
