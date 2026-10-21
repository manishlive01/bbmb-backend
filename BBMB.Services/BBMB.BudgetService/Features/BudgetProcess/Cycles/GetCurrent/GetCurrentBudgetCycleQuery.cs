using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetCurrent
{
    public sealed record GetCurrentBudgetCycleQuery : IRequest<Response<BudgetCycleResponseDto>>;
}
