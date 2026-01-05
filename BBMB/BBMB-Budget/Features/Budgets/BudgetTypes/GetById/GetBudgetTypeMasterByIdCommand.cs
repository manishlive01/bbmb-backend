using BBMB_Budget.Features.Budgets.BudgetTypes.GetById.Dtos;
using BBMB_Budget.Shared.RequestDTOs;
using MediatR;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.GetById
{
    public sealed record GetBudgetTypeMasterByIdCommand(GetByIdRequestDto Request) : IRequest<GetBudgetTypeMasterByIdResponseDto?>;
}
