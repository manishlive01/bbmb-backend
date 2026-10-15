using BBMB.BudgetService.Features.BudgetProcess.Freezes.Freeze.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Freezes.Freeze
{
    /// <summary>
    /// Design §8.6 POST freeze. Success: <see cref="Response"/>; list refusal:
    /// <c>Response&lt;FreezeRefusalDto&gt;</c> (M-19); redistribution refusal: a plain M-30; any other refusal a
    /// plain catalogued <see cref="Response"/>.
    /// </summary>
    public sealed record FreezeCommand(long CycleId, FreezeRequestDto Dto) : IRequest<Response>;
}
