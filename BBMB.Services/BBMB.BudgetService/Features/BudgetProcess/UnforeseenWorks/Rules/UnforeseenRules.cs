using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Rules
{
    /// <summary>
    /// Design §8.6 authorise (VR-13 / M-29, VR-04 / M-09; BUD-295, BUD-297, BUD-299). The application pre-checks
    /// the Chief Engineer's authorisation makes before the T-50 insert; VR-13 is also enforced at the insert by
    /// the DB-26 trigger, and the balanced movements by DB-21.
    /// </summary>
    public sealed class UnforeseenRules(BudgetContext context)
    {
        /// <summary>True when the actor's office is the CE whose charge the request's Division falls under (DB-01, BUD-295).</summary>
        public async Task<bool> ActorIsCeOfChargeAsync(long actorOfficeId, long divisionOfficeId, CancellationToken ct = default)
        {
            var actor = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == actorOfficeId && o.OfficeLevel == OfficeLevel.Ce)
                .Select(o => (long?)o.OfficeId).FirstOrDefaultAsync(ct);
            if (actor is null)
                return false;
            // The Division's Circle's CE must be the acting CE office.
            var ceOfDivision = await (
                from division in context.BudgetOffices.AsNoTracking()
                where division.OfficeId == divisionOfficeId
                join circle in context.BudgetOffices.AsNoTracking() on division.ParentOfficeId equals circle.OfficeId
                select circle.ParentOfficeId).FirstOrDefaultAsync(ct);
            return ceOfDivision == actorOfficeId;
        }

        /// <summary>The available balance on a provision (V-02); 0 when none/absent.</summary>
        public async Task<decimal> BalanceAsync(long? provisionId, CancellationToken ct = default) =>
            provisionId is null
                ? 0
                : await context.ProvisionBalances.AsNoTracking()
                    .Where(v => v.ProvisionId == provisionId.Value).Select(v => (decimal?)v.Available).FirstOrDefaultAsync(ct) ?? 0;

        /// <summary>VR-13 (M-29): an authorised amount is not above the balance of the provision drawn upon.</summary>
        public async Task<bool> Vr13HoldsAsync(long? provisionId, decimal? amount, CancellationToken ct = default)
        {
            if (amount is null)
                return true;
            return amount.Value <= await BalanceAsync(provisionId, ct);
        }

        /// <summary>
        /// VR-04 (M-09, BUD-299): the total authorised does not exceed <c>AA_DEMAND_CEILING_PCT</c> of the
        /// work's current administrative-approval amount. No approval or no ceiling → the rule does not bite.
        /// </summary>
        public async Task<bool> Vr04HoldsAsync(int workId, decimal total, CancellationToken ct = default)
        {
            var aaAmount = await (
                from a in context.AdminApprovals.AsNoTracking()
                where a.WorkId == workId
                join v in context.AdminApprovalVersions.AsNoTracking()
                    on new { a.AdminApprovalId, Ver = a.CurrentVersionNo } equals new { v.AdminApprovalId, Ver = v.VersionNo }
                select (decimal?)v.AmountSanctioned).FirstOrDefaultAsync(ct);
            if (aaAmount is not { } aa || aa <= 0)
                return true;
            var reader = new Settings.BudgetSettingsReader(context);
            var pct = (await reader.GetAsync(Settings.Entities.SettingKey.AaDemandCeilingPct, DateTimeOffset.UtcNow, ct))?.NumericValue;
            if (pct is null)
                return true;
            return total * 100 <= pct.Value * aa;
        }
    }
}
