using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.Rules
{
    /// <summary>
    /// The work-master rules of design §5.2, §5.3 and §11.3, implemented once and used by the Works slices and
    /// by <c>PUT /api/works/{id}/legacy-code</c> (P4).
    /// </summary>
    public sealed class WorkRules(BudgetContext context)
    {
        /// <summary>
        /// First-cycle guard (design §5.3). Reads the first cycle directly, not the current cycle, so a later OPEN
        /// cycle cannot make it pass or fail by accident: the <c>is_first_cycle = true</c> T-14 row exists and is
        /// OPEN, and the Division's WORKS proposal in it is absent or has <c>first_submitted_at IS NULL</c>.
        /// Returns null when the guard passes, else <see cref="BudgetMessages.M19"/>.
        /// </summary>
        public async Task<string?> CheckFirstCycleAsync(long divisionOfficeId, CancellationToken ct = default)
        {
            var firstCycle = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.IsFirstCycle)
                .Select(c => new { c.CycleId, c.Status })
                .FirstOrDefaultAsync(ct);
            if (firstCycle is null || firstCycle.Status != BudgetProcess.Cycles.Entities.CycleStatus.Open)
                return BudgetMessages.M19;

            var submitted = await context.BudgetProposals.AsNoTracking().AnyAsync(p =>
                p.CycleId == firstCycle.CycleId &&
                p.Category == ProposalCategory.Works &&
                p.OriginOfficeId == divisionOfficeId &&
                p.FirstSubmittedAt != null, ct);
            return submitted ? BudgetMessages.M19 : null;
        }

        /// <summary>
        /// True when a record the work's head and class are part of references it (design §11.3 reference rule).
        /// A non-deleted T-30 demand line (P1), a T-25 administrative approval or a T-27 technical sanction (P2,
        /// whose versions carry the head and whose tab is the class). P4 extends this with T-41.
        /// </summary>
        public async Task<bool> IsReferencedAsync(int workId, CancellationToken ct = default) =>
            await context.DemandLines.AsNoTracking().AnyAsync(l => l.WorkId == workId && !l.IsDeleted, ct)
            || await context.AdminApprovals.AsNoTracking().AnyAsync(a => a.WorkId == workId, ct)
            || await context.TechnicalSanctions.AsNoTracking().AnyAsync(t => t.WorkId == workId, ct)
            // P4 (design §11.3): a provision now also fixes the work's head and class.
            || await context.Provisions.AsNoTracking().AnyAsync(p => p.WorkId == workId, ct);

        /// <summary>
        /// Resolves a T-07 <c>account_code_id</c> for use on a work (design §5.2): absent or <c>not_for_use</c>
        /// → <see cref="BudgetMessages.M05"/> (one id for both, catalogue gap OI-13).
        /// </summary>
        public async Task<(HeadOfAccount? Code, string? MessageId)> ResolveAccountCodeAsync(long accountCodeId, CancellationToken ct = default)
        {
            var code = await context.HeadOfAccounts.AsNoTracking()
                .FirstOrDefaultAsync(h => h.AccountCodeId == accountCodeId, ct);
            return code is null || code.NotForUse ? (null, BudgetMessages.M05) : (code, null);
        }

        /// <summary>
        /// The master Division ids whose works the actor may see (design §9.3 Works reads, BUD-031): the
        /// DIVISION offices among <see cref="IBudgetOfficeScope.GetVisibleOfficeIdsAsync"/>. No actor → none.
        /// </summary>
        public async Task<IReadOnlyList<int>> GetVisibleDivisionIdsAsync(
            IBudgetActorSource actorSource, IBudgetOfficeScope officeScope, CancellationToken ct = default)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return [];
            var visible = (await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).ToList();
            return await context.BudgetOffices.AsNoTracking()
                .Where(o => visible.Contains(o.OfficeId) && o.OfficeLevel == OfficeLevel.Division && o.DivisionId != null)
                .Select(o => o.DivisionId!.Value)
                .ToListAsync(ct);
        }

        // NextInterimWorkCodeAsync removed in P4 (design §4.6, DV-19): a work is created with NO code. The
        // four creation paths (F-03 AA_ENTRY, F-06 AMR TS_ENTRY, F-07 DEMAND_ENTRY, and the upload demand
        // path) now set work_code = null. A code is generated only on the four allocation occasions
        // (BUD-232) by budget.fn_generate_work_code, or keyed as a legacy code (F-28).

        /// <summary>Copies the six segments of a resolved T-07 code onto the work (design §5.2).</summary>
        public static void ApplySegments(Work work, HeadOfAccount code)
        {
            work.MajorAccCode = code.MajorAccCode;
            work.SubMajorAccCode = code.SubMajorAccCode ?? string.Empty;
            work.MinorAccCode = code.MinorAccCode;
            work.SubMinorAccCode = code.SubMinorAccCode ?? string.Empty;
            work.DetailAccCode = code.DetailAccCode;
            work.ObjectAccCode = code.ObjectAccCode;
        }

        /// <summary>True when the work already carries the six segments of <paramref name="code"/>.</summary>
        public static bool HasSegments(Work work, HeadOfAccount code) =>
            work.MajorAccCode == code.MajorAccCode &&
            work.SubMajorAccCode == (code.SubMajorAccCode ?? string.Empty) &&
            work.MinorAccCode == code.MinorAccCode &&
            work.SubMinorAccCode == (code.SubMinorAccCode ?? string.Empty) &&
            work.DetailAccCode == code.DetailAccCode &&
            work.ObjectAccCode == code.ObjectAccCode;
    }
}
