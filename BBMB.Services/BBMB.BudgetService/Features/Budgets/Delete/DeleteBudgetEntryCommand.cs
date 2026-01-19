using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.Delete
{
    public sealed record DeleteBudgetEntryCommand(int Id) : IRequest<Response>;
}
