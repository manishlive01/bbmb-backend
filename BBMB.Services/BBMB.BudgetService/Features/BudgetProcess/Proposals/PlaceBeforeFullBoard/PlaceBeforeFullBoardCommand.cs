using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeFullBoard
{
    /// <summary>
    /// Design §6.2 T25, §8.6 place-before-full-board. Success / list refusal:
    /// <c>Response&lt;BulkTransitionResponseDto&gt;</c>; any other refusal: a plain catalogued <see cref="Response"/>.
    /// </summary>
    public sealed record PlaceBeforeFullBoardCommand(long CycleId) : IRequest<Response>;
}
