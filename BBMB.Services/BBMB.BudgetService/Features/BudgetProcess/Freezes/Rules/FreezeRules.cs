using BBMB.BudgetService.Features.BudgetProcess.Allocations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Freezes.Rules
{
    /// <summary>
    /// Design §8.6 freeze, DDD T-44 / DB-19 (BUD-190, BUD-193, M-30). The three preconditions of a freeze,
    /// pre-checked so the response is a catalogued message with the outstanding list rather than a raw trigger
    /// error; the DB trigger fn_freeze_preconditions enforces the same on the T-44 insert.
    /// </summary>
    public sealed class FreezeRules(BudgetContext context)
    {
        public sealed record FreezeCheck(string? MessageId, IReadOnlyList<OutstandingFreezeProposal> Outstanding);

        public sealed record OutstandingFreezeProposal(long ProposalId, long OriginOfficeId, string StateCode);

        /// <summary>Null MessageId → the freeze may proceed. Otherwise the catalogued refusal and any list.</summary>
        public async Task<FreezeCheck> CheckAsync(long cycleId, CancellationToken ct = default)
        {
            // (1) the FULL_BOARD allocation of the cycle exists (BUD-190).
            if (!await context.Allocations.AsNoTracking()
                    .AnyAsync(a => a.CycleId == cycleId && a.Body == AllocationBody.FullBoard, ct))
                return new FreezeCheck(BudgetMessages.M19, []);

            // (2) no recommendation of the cycle still requires redistribution (M-30).
            if (await context.CeRecommendations.AsNoTracking()
                    .AnyAsync(r => r.CycleId == cycleId && r.RedistributionRequired, ct))
                return new FreezeCheck(BudgetMessages.M30, []);

            // (3) every proposal of the cycle stands at S-15 (BUD-193); else list the ones that do not.
            var outstanding = await context.BudgetProposals.AsNoTracking()
                .Where(p => p.CycleId == cycleId && p.StateCode != ProposalStateCode.S15)
                .OrderBy(p => p.ProposalId)
                .Select(p => new OutstandingFreezeProposal(p.ProposalId, p.OriginOfficeId, p.StateCode))
                .ToListAsync(ct);
            if (outstanding.Count > 0)
                return new FreezeCheck(BudgetMessages.M19, outstanding);

            return new FreezeCheck(null, []);
        }

        /// <summary>The proposals to move FREEZE (every one of the cycle, all at S-15 once the check passes).</summary>
        public async Task<IReadOnlyList<long>> ProposalIdsAsync(long cycleId, CancellationToken ct = default) =>
            await context.BudgetProposals.AsNoTracking()
                .Where(p => p.CycleId == cycleId)
                .OrderBy(p => p.ProposalId).Select(p => p.ProposalId).ToListAsync(ct);
    }
}
