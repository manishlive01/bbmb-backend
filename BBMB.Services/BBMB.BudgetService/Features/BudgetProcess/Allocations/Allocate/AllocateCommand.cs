using BBMB.BudgetService.Features.BudgetProcess.Allocations.Allocate.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Allocate
{
    /// <summary>
    /// Design §8.6 POST allocations. Success: <c>Response&lt;AllocationResponseDto&gt;</c>; preview-mismatch
    /// refusal: <c>Response&lt;AllocationPreviewMismatchDto&gt;</c> (M-19); any other refusal: a plain
    /// catalogued <see cref="Response"/>.
    /// </summary>
    public sealed record AllocateCommand(long CycleId, AllocateRequestDto Dto) : IRequest<Response>;
}
