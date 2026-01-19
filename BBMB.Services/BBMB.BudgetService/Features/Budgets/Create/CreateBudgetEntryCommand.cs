using BBMB.BudgetService.Features.Budgets.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.Create
{
    public sealed record CreateBudgetEntryCommand(CreateBudgetEntryDto dto) : IRequest<Response>;
}
