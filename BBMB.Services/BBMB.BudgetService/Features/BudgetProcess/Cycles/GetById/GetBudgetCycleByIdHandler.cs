using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.GetById
{
    /// <summary>A cycle is not office-scoped: every office works in the same cycle (DDD T-14).</summary>
    public sealed class GetBudgetCycleByIdHandler(BudgetContext context)
        : IRequestHandler<GetBudgetCycleByIdQuery, Response<BudgetCycleResponseDto>>
    {
        public async Task<Response<BudgetCycleResponseDto>> Handle(GetBudgetCycleByIdQuery request, CancellationToken ct)
        {
            var cycle = await context.BudgetCycles.AsNoTracking().Where(c => c.CycleId == request.CycleId)
                .Select(BudgetCycleProjection.ToDto).FirstOrDefaultAsync(ct);
            return cycle is null
                ? BudgetResponses.Fail<BudgetCycleResponseDto>(BudgetMessages.NotFound)
                : new Response<BudgetCycleResponseDto> { IsSucced = true, Message = "Budget cycle retrieved", Data = cycle };
        }
    }
}
