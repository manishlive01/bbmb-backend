using BBMB.BudgetService.Features.Budgets.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.Update
{
    public sealed record UpdateBudgetEntryCommand(int Id, UpdateBudgetEntryDto dto) : IRequest<Response>;
}
