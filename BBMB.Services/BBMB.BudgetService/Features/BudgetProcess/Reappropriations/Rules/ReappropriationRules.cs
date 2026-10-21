using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Rules
{
    /// <summary>
    /// Design §8.7 F-21 (VR-08, M-18; BUD-199, BUD-200). The surrender arithmetic of a re-appropriation: the
    /// cumulative of the non-removed sources, the amount still to surrender, and whether a proposed cumulative
    /// stays within the additional amount. DB-24 enforces the same bound at commit.
    /// </summary>
    public sealed class ReappropriationRules(BudgetContext context)
    {
        /// <summary>The sum of the entry's non-removed surrender sources.</summary>
        public async Task<decimal> SurrenderedAsync(long reappropriationId, long? excludingSourceId = null, CancellationToken ct = default) =>
            await context.ReappropriationSources.AsNoTracking()
                .Where(s => s.ReappropriationId == reappropriationId && !s.IsRemoved &&
                            (excludingSourceId == null || s.ReappropriationSourceId != excludingSourceId))
                .SumAsync(s => (decimal?)s.Amount, ct) ?? 0;

        /// <summary>
        /// The amount still to surrender for the entry to balance (additional − cumulative). Negative is clamped
        /// to zero; a positive value is what the officer must still surrender (BUD-200).
        /// </summary>
        public static decimal ToSurrender(decimal additional, decimal cumulative) => Math.Max(0, additional - cumulative);

        /// <summary>VR-08 (M-18): a proposed cumulative surrender does not exceed the additional amount (BUD-199).</summary>
        public static bool WithinAdditional(decimal additional, decimal cumulative) => cumulative <= additional;
    }
}
