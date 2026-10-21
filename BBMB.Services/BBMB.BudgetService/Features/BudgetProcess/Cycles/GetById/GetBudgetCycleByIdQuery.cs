using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetById
{
    public sealed record GetBudgetCycleByIdQuery(long CycleId) : IRequest<Response<BudgetCycleResponseDto>>;
}
