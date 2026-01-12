using BBMB.BudgetService.Features.Budgets.BudgetTypes.GetById.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetById
{
    public sealed record GetBudgetTypeMasterByIdCommand(GetByIdRequestDto Request) : IRequest<GetBudgetTypeMasterByIdResponseDto?>;
}
