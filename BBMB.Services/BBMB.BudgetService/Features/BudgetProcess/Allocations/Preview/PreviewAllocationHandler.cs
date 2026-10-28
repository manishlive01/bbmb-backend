using BBMB.BudgetService.Features.BudgetProcess.Allocations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Allocations.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview
{
    /// <summary>
    /// Design §8.6 allocations/preview. Shows what the body's allocation would activate (BUD-189). Refuses with
    /// the outstanding list when a proposal of the cycle is not yet at the placement state (S-12 for BSC, S-14
    /// for FULL_BOARD). A read: no provision is written. Right F19.Allocate (endpoint policy).
    /// </summary>
    public sealed class PreviewAllocationHandler(
        BudgetContext context, IBudgetActorSource actorSource, AllocationRules rules)
        : IRequestHandler<PreviewAllocationQuery, Response>
    {
        public async Task<Response> Handle(PreviewAllocationQuery request, CancellationToken ct)
        {
            if (request.Body is not (AllocationBody.Bsc or AllocationBody.FullBoard))
                return BudgetResponses.Fail(BudgetMessages.M19);

            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == request.CycleId).Select(c => c.Status).FirstOrDefaultAsync(ct);
            if (cycleStatus is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M17);

            var computation = await rules.ComputeAsync(request.CycleId, request.Body, ct);
            if (computation is null)
            {
                var requiredState = AllocationRules.StateFor(request.Body);
                var outstanding = await context.BudgetProposals.AsNoTracking()
                    .Where(p => p.CycleId == request.CycleId && p.StateCode != requiredState)
                    .OrderBy(p => p.ProposalId)
                    .Select(p => new OutstandingProposalDto(p.ProposalId, p.OriginOfficeId, p.StateCode))
                    .ToListAsync(ct);
                return BudgetResponses.Fail(BudgetMessages.M19, new AllocationPreviewDto { Body = request.Body, Outstanding = outstanding });
            }

            var dto = new AllocationPreviewDto
            {
                Body = request.Body,
                LinesToActivate = computation.LinesToActivate,
                TotalToActivate = computation.TotalToActivate,
                WorksToReceiveCode = computation.WorksToReceiveCode.OrderBy(w => w).ToList(),
                ByCategory = computation.Provisions
                    .GroupBy(p => p.Category)
                    .OrderBy(g => g.Key)
                    .Select(g => new AllocationCategoryPreviewDto(g.Key, g.Count(), g.Sum(x => x.Amount)))
                    .ToList(),
            };
            return new Response<AllocationPreviewDto> { IsSucced = true, Message = "Allocation preview", Data = dto };
        }
    }
}
