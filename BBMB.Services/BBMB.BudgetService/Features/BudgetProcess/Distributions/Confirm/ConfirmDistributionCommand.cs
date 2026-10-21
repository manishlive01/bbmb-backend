using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Confirm
{
    /// <summary>
    /// Design §8.5 (<c>POST /api/budget-cycles/{cycleId}/distributions/{recommendationId}/confirm</c>, BUD-280):
    /// the CE confirms the distribution, clearing <c>redistribution_required</c> only when VR-07 holds.
    /// </summary>
    public sealed record ConfirmDistributionCommand(long CycleId, long RecommendationId) : IRequest<Response>;
}
