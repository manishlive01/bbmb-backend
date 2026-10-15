using BBMB.BudgetService.Features.BudgetProcess.Cycles.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.Create
{
    public sealed record CreateBudgetCycleCommand(CreateBudgetCycleDto Dto) : IRequest<Response<long>>;
}
