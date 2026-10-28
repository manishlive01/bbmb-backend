using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview
{
    /// <summary>
    /// Design §8.6 allocations preview, served as POST /api/budget-cycles/allocations/preview with
    /// <c>{ cycleId, body }</c> in the request body (no ids in the URL). Success / list refusal:
    /// <c>Response&lt;AllocationPreviewDto&gt;</c>; any other refusal: a plain catalogued <see cref="Response"/>.
    /// </summary>
    public sealed record PreviewAllocationQuery(long CycleId, string Body) : IRequest<Response>;
}
