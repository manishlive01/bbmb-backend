using BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe
{
    /// <summary>
    /// Design §8.5 (<c>GET /api/budget-cycles/{cycleId}/distributions?category=&amp;ceOfficeId=</c>): per head
    /// the recommended figure, the share distributed to each office, and the balance retained (V-05, BUD-160).
    /// </summary>
    public sealed record GetDistributionsQuery(long CycleId, string Category, long CeOfficeId)
        : IRequest<Response<List<DistributionHeadDto>>>;
}
