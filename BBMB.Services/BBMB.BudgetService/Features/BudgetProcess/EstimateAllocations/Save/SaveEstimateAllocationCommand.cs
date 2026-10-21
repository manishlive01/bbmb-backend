using BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Save.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Save
{
    public sealed record SaveEstimateAllocationCommand(long ProvisionId, SaveEstimateAllocationDto Dto) : IRequest<Response>;
}
