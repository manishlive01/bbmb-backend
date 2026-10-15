using BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.GetForDivision.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.GetForDivision
{
    public sealed record GetForDivisionQuery(int DivisionId, int FinYear) : IRequest<List<EstimateAllocationWorkDto>>;
}
