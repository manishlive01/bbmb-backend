using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Freezes.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Freezes.Freeze.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Freezes.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Freezes.Freeze
{
    /// <summary>
    /// Design §8.6 freeze (F-20, DDD T-44 / DB-19; BUD-190..193). Order, nothing written on any refusal:
    /// <list type="number">
    /// <item>actor (M-20); cycle absent → NOT_FOUND; already frozen → M-19;</item>
    /// <item><see cref="FreezeRules.CheckAsync"/>: FULL_BOARD allocation exists (M-19), no redistribution flag
    /// (M-30), every proposal at S-15 (M-19 with the outstanding list);</item>
    /// <item>the T-44 row (the DB-19 trigger sets the cycle FROZEN), then every proposal moves FREEZE to S-16.
    /// There is no unfreeze route (BUD-193).</item>
    /// </list>
    /// The FREEZE transitions run after the cycle is FROZEN; the transition service exempts FREEZE from its
    /// cycle-OPEN check for exactly this reason.
    /// </summary>
    public sealed class FreezeHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope,
        FreezeRules rules, IProposalTransitionService transitions)
        : IRequestHandler<FreezeCommand, Response>
    {
        public async Task<Response> Handle(FreezeCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var cycle = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == request.CycleId).Select(c => c.Status).FirstOrDefaultAsync(ct);
            if (cycle is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (cycle != CycleStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M19); // already frozen or closed; no unfreeze

            var check = await rules.CheckAsync(request.CycleId, ct);
            if (check.MessageId is not null)
            {
                if (check.Outstanding.Count > 0)
                    return BudgetResponses.Fail(check.MessageId, new FreezeRefusalDto
                    {
                        Outstanding = check.Outstanding
                            .Select(o => new OutstandingFreezeDto(o.ProposalId, o.OriginOfficeId, o.StateCode)).ToList(),
                    });
                return BudgetResponses.Fail(check.MessageId);
            }

            var proposalIds = await rules.ProposalIdsAsync(request.CycleId, ct);

            await using (var tx = await scope.BeginAsync("FREEZE", onBehalfOfPost: "FA_CAO", ct: ct))
            {
                context.BudgetFreezes.Add(new BudgetFreeze
                {
                    CycleId = request.CycleId,
                    FaCaoApprovalRef = request.Dto.FaCaoApprovalRef,
                    Remarks = request.Dto.Remarks,
                    RoleCode = actor.RoleCode,
                    OnBehalfOfPost = "FA_CAO",
                });
                await context.SaveChangesAsync(ct); // DB-19 trigger sets the cycle FROZEN
                await tx.CommitAsync(ct);
            }

            // FREEZE every proposal to S-16 (the cycle is now FROZEN; the service exempts FREEZE).
            var moved = await transitions.MoveAllAsync(proposalIds, ProposalAction.Freeze, null, ct);
            if (!moved.IsSucced)
                return moved;

            return new Response { IsSucced = true, Message = "Cycle frozen" };
        }
    }
}
