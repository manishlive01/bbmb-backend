using BBMB.BudgetService.Features.Budgets.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.GetById
{
    public sealed record GetBudgetEntryByIdQuery(int Id) : IRequest<GetBudgetEntryByIdResponseDto>;
}
