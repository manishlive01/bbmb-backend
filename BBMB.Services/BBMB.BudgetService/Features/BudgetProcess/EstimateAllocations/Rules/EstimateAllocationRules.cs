using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Rules
{
    /// <summary>
    /// Design §8.6 F-29 (VR-12, M-28; BUD-288..291, BUD-309). The application pre-checks of estimate-wise
    /// allocation: the destination provision is a live WORK provision of the actor's Division and is allocated;
    /// the sanctions are of the same work and carry an estimate code (DB-04); the total does not exceed the
    /// provision's available amount (VR-12 / M-28, also enforced at commit by DB-23); and no critical correction
    /// flag stands against the work (BUD-309).
    /// </summary>
    public sealed class EstimateAllocationRules(BudgetContext context)
    {
        public sealed record ProvisionTarget(long ProvisionId, int WorkId, long OfficeId, decimal Available);

        /// <summary>
        /// Resolves the destination provision and returns it with its available amount, or a catalogued failure:
        /// not found / not a live allocated WORK provision → NOT_FOUND; not of the actor's Division → NOT_FOUND
        /// (not disclosed); a critical flag stands → M-19.
        /// </summary>
        public async Task<(ProvisionTarget? Target, string? MessageId)> ResolveTargetAsync(
            long provisionId, int actorDivisionId, CancellationToken ct = default)
        {
            var provision = await context.Provisions.AsNoTracking()
                .Where(p => p.ProvisionId == provisionId && p.HolderType == ProvisionHolderType.Work &&
                            p.SupersededByProvisionId == null && p.AllocationId != null && p.WorkId != null)
                .Select(p => new { p.ProvisionId, WorkId = p.WorkId!.Value, p.OfficeId })
                .FirstOrDefaultAsync(ct);
            if (provision is null)
                return (null, BudgetMessages.NotFound);

            // The provision's office is the Division of the work (DB-04); only the actor's Division may allocate.
            var provisionDivisionId = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == provision.OfficeId).Select(o => o.DivisionId).FirstOrDefaultAsync(ct);
            if (provisionDivisionId != actorDivisionId)
                return (null, BudgetMessages.NotFound);

            // A critical correction flag against the work blocks F-29 for the work (BUD-309).
            if (await context.CorrectionFlags.AsNoTracking().AnyAsync(f =>
                    (f.WorkId == provision.WorkId || f.ProvisionId == provision.ProvisionId) &&
                    f.IsCritical && f.ClearedAt == null, ct))
                return (null, BudgetMessages.M19);

            var available = await context.ProvisionBalances.AsNoTracking()
                .Where(v => v.ProvisionId == provisionId).Select(v => (decimal?)v.Available).FirstOrDefaultAsync(ct) ?? 0;

            return (new ProvisionTarget(provision.ProvisionId, provision.WorkId, provision.OfficeId, available), null);
        }

        /// <summary>
        /// The technical sanctions of the work that carry an estimate code (DB-04): the only sanctions an
        /// estimate allocation may name. Keyed by technical_sanction_id.
        /// </summary>
        public async Task<IReadOnlyDictionary<long, string>> SanctionsWithEstimateCodeAsync(int workId, CancellationToken ct = default) =>
            await context.TechnicalSanctions.AsNoTracking()
                .Where(t => t.WorkId == workId && t.EstimateCode != null)
                .ToDictionaryAsync(t => t.TechnicalSanctionId, t => t.EstimateCode!, ct);

        /// <summary>True when VR-12 holds: the estimate allocation of the work's provision now equals (uses up) its available.</summary>
        public static bool Vr12Satisfied(decimal totalAllocated, decimal available) => totalAllocated == available;

        public string ReallocateEstimatesReason => CorrectionFlagReason.ReallocateEstimates;
    }
}
