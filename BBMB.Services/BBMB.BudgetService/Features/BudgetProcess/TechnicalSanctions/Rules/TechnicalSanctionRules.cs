using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.FinancialAccounting;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Rules
{
    /// <summary>
    /// Application rules of F-06 (design §8.4 technical-sanction rows, §7 VR-02, VR-11). DB-04, DB-07 and DB-09
    /// stay in the database as backstops; the pre-checks here take the same advisory-lock keys as the triggers, so
    /// the application answer and the commit-time answer cannot disagree under concurrency.
    /// </summary>
    public sealed class TechnicalSanctionRules(
        BudgetContext context, IBudgetSettingsReader settings, IFinancialAccountingGateway financialAccounting, TimeProvider? clock = null)
    {
        /// <summary>
        /// VR-02 (BUD-072, BUD-073; M-06, no override): Σ current versions of the approval's AA-tab sanctions,
        /// with <paramref name="excludeSanctionId"/>'s current version replaced by <paramref name="newAmount"/> (a
        /// revision) or <paramref name="newAmount"/> added (a new sanction), is at most TS_CEILING_PCT (T-17, in
        /// force now) of the approval's current amount; compared as sum × 100 ≤ pct × amount, so exactly 105 % passes.
        /// Takes the DB-09 lock key <c>pg_advisory_xact_lock(hashtext('budget.ts_ceiling'), (id % 2147483647)::int)</c>
        /// (BudgetP2TsCeilingLock), so it must run inside the action scope. No ceiling in force → refused (fail closed).
        /// Returns null when the ceiling holds, else M-06.
        /// </summary>
        public async Task<string?> CheckAaCeilingAsync(long adminApprovalId, decimal newAmount, long? excludeSanctionId, CancellationToken ct = default)
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtext('budget.ts_ceiling'), ({adminApprovalId} % 2147483647)::int)", ct);
            var aaAmount = await context.AdminApprovals.AsNoTracking()
                .Where(a => a.AdminApprovalId == adminApprovalId)
                .Join(context.AdminApprovalVersions, a => new { a.AdminApprovalId, V = a.CurrentVersionNo },
                    v => new { v.AdminApprovalId, V = v.VersionNo }, (a, v) => (decimal?)v.AmountSanctioned)
                .FirstOrDefaultAsync(ct);
            var others = await context.TechnicalSanctions.AsNoTracking()
                .Where(t => t.AdminApprovalId == adminApprovalId && t.Tab == "AA" && t.TechnicalSanctionId != excludeSanctionId)
                .Join(context.TechnicalSanctionVersions, t => new { t.TechnicalSanctionId, V = t.CurrentVersionNo },
                    v => new { v.TechnicalSanctionId, V = v.VersionNo }, (t, v) => v.TsAmount)
                .SumAsync(ct);
            var pct = (await settings.GetAsync(SettingKey.TsCeilingPct, (clock ?? TimeProvider.System).GetUtcNow(), ct))?.NumericValue;
            if (aaAmount is null || pct is null || (others + newAmount) * 100 > pct.Value * aaAmount.Value)
                return BudgetMessages.M06;
            return null;
        }

        /// <summary>The DIVISION office of the actor with its master Division, or null when the actor is not at a Division.</summary>
        public async Task<(long OfficeId, int DivisionId)?> ActorDivisionAsync(BudgetActor actor, CancellationToken ct = default)
        {
            if (actor.OfficeLevel != Offices.Entities.OfficeLevel.Division)
                return null;
            var division = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == actor.OfficeId && o.OfficeLevel == Offices.Entities.OfficeLevel.Division)
                .Select(o => o.DivisionId).FirstOrDefaultAsync(ct);
            return division is { } d ? (actor.OfficeId, d) : null;
        }

        /// <summary>VR-10 (M-07): the copy exists, is of evidence class (DB-08) and was uploaded by the actor.</summary>
        public Task<bool> IsUsableEvidenceAsync(long attachmentId, BudgetActor actor, CancellationToken ct = default) =>
            context.BudgetAttachments.AsNoTracking().AnyAsync(a =>
                a.AttachmentId == attachmentId && a.EvidenceClass == AttachmentEvidenceClass.Evidence &&
                a.CreatedBy == actor.EmployeeId, ct);

        /// <summary>DB-07 pre-check: the TS number is used by another sanction of the Division (own revisions may repeat it).</summary>
        public Task<bool> IsTsNumberUsedAsync(int divisionId, string tsNumber, long? ownSanctionId, CancellationToken ct = default) =>
            context.TechnicalSanctionVersions.AsNoTracking()
                .Where(v => v.TsNumber == tsNumber && v.TechnicalSanctionId != ownSanctionId)
                .Join(context.TechnicalSanctions, v => v.TechnicalSanctionId, t => t.TechnicalSanctionId, (v, t) => t.WorkId)
                .Join(context.Works, w => w, w => w.Id, (w, work) => work.DivisionId)
                .AnyAsync(d => d == divisionId, ct);

        /// <summary>
        /// The next sanction serial in the Division (DDD T-27 UK (Division, serial)), under the DB-04 trigger's
        /// lock key <c>hashtext('budget.ts_serial'), division</c>. Call inside the action scope.
        /// </summary>
        public async Task<int> NextSerialAsync(int divisionId, CancellationToken ct = default)
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtext('budget.ts_serial'), {divisionId})", ct);
            return (await context.TechnicalSanctions.AsNoTracking()
                .Join(context.Works, t => t.WorkId, w => w.Id, (t, w) => new { t.SerialNo, w.DivisionId })
                .Where(x => x.DivisionId == divisionId).MaxAsync(x => (int?)x.SerialNo, ct) ?? 0) + 1;
        }

        /// <summary>
        /// VR-11 (BUD-273, M-31): a revised amount is not below the expenditure booked against the sanction's estimate
        /// code. Not applied (and said so) while Financial Accounting is unavailable (OI-15) or before an estimate code
        /// is recorded (nothing can be booked against a code that does not exist).
        /// </summary>
        public async Task<(string? MessageId, string? NotApplied)> CheckBookedAsync(string? estimateCode, decimal newAmount, CancellationToken ct = default)
        {
            if (estimateCode is null)
                return (null, "VR-11 not applied: no estimate code is recorded for this sanction.");
            var booked = await financialAccounting.GetBookedAgainstEstimateCodeAsync(estimateCode, ct);
            if (!booked.Available || booked.Amount is null)
                return (null, "VR-11 not applied: Financial Accounting is not available (OI-15).");
            return newAmount < booked.Amount.Value ? (BudgetMessages.M31, null) : (null, null);
        }
    }
}
