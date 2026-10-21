using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetCurrent
{
    /// <summary>Design §5.3 current cycle through <see cref="BudgetCycleQueries.GetCurrentAsync"/>; none → NOT_FOUND.</summary>
    public sealed class GetCurrentBudgetCycleHandler(BudgetContext context)
        : IRequestHandler<GetCurrentBudgetCycleQuery, Response<BudgetCycleResponseDto>>
    {
        public async Task<Response<BudgetCycleResponseDto>> Handle(GetCurrentBudgetCycleQuery request, CancellationToken ct)
        {
            var c = await BudgetCycleQueries.GetCurrentAsync(context, ct);
            return c is null
                ? BudgetResponses.Fail<BudgetCycleResponseDto>(BudgetMessages.NotFound)
                : new Response<BudgetCycleResponseDto> { IsSucced = true, Message = "Current budget cycle", Data = BudgetCycleProjection.ToDto.Compile()(c) };
        }
    }
}
