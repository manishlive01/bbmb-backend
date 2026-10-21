using BBMB.BudgetService.Features.BudgetProcess.Distributions.Save.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Save
{
    public sealed record SaveDistributionCommand(long CycleId, SaveDistributionDto Dto) : IRequest<Response>;
}
