using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeBsc
{
    /// <summary>
    /// Design §6.2 T22, §8.6 place-before-bsc. Success / list refusal: <c>Response&lt;BulkTransitionResponseDto&gt;</c>;
    /// any other refusal: a plain catalogued <see cref="Response"/>.
    /// </summary>
    public sealed record PlaceBeforeBscCommand(long CycleId) : IRequest<Response>;
}
