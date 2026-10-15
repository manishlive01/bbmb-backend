using Auth.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines
{
    /// <summary>The proposal a demand-line write may proceed against, or the catalogued refusal.</summary>
    public sealed record DemandLineWriteTarget(BudgetActor? Actor, BudgetProposal? Proposal, Response? Refusal);

    /// <summary>
    /// The checks every origin-office demand-line write runs, in the design §4.9 order, and the shared steps
    /// of a Save (design §6.4, §8.3, §4.10):
    /// <list type="number">
    /// <item>no actor → M-20; proposal absent or not visible → NOT_FOUND (BUD-031);</item>
    /// <item>the category's entry right (<c>F07.Enter</c> WORKS, <c>F11.Enter</c> head-wise) through
    /// <see cref="IAuthorizationService"/> → M-20 (design §8.1);</item>
    /// <item>the actor's office is not the origin office → M-20 (§4.9: visible, other write, no authority);</item>
    /// <item>the state is not S-01 / S-04 held by the origin → M-19 (BUD-095; §8.3 "elsewhere M-19");</item>
    /// <item>the cycle is not OPEN → M-17 (BUD-191; DB-11 is the backstop);</item>
    /// <item>another officer's live edit lease → M-23 (BUD-015). Otherwise the write renews the lease.</item>
    /// </list>
    /// </summary>
    public static class DemandLineWrites
    {
        public const string AddAction = "DEMAND_LINE_ADD";
        public const string UpdateAction = "DEMAND_LINE_UPDATE";
        public const string DeleteAction = "DEMAND_LINE_DELETE";

        /// <summary>
        /// The legacy NOT NULL <c>work.user_code</c> of a work created by demand entry. Not a DDD actor column:
        /// the officer is recorded in <c>created_by</c> / <c>created_charge_id</c> by <c>fn_require_actor</c>.
        /// </summary>
        public const string DemandEntryUserCode = "BUDDE";

        /// <summary>The entry right of the category (design §8.3 "F07.Enter / F11.Enter").</summary>
        public static string EntryRightFor(string category) =>
            category == ProposalCategory.Works ? "F07.Enter" : "F11.Enter";

        public static async Task<DemandLineWriteTarget> ResolveAsync(
            BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope,
            IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor,
            long proposalId, DateTimeOffset now, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return new(null, null, BudgetResponses.Fail(BudgetMessages.M20));
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, proposalId, true, ct);
            if (p is null)
                return new(actor, null, BudgetResponses.Fail(BudgetMessages.NotFound));
            var user = httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            if (!(await authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy(EntryRightFor(p.Category)))).Succeeded)
                return new(actor, p, BudgetResponses.Fail(BudgetMessages.M20));
            if (actor.OfficeId != p.OriginOfficeId)
                return new(actor, p, BudgetResponses.Fail(BudgetMessages.M20));
            if (p.StateCode is not (ProposalStateCode.S01 or ProposalStateCode.S04) || p.HolderOfficeId != p.OriginOfficeId)
                return new(actor, p, BudgetResponses.Fail(BudgetMessages.M19));
            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == p.CycleId).Select(c => c.Status).SingleAsync(ct);
            if (cycleStatus != CycleStatus.Open)
                return new(actor, p, BudgetResponses.Fail(BudgetMessages.M17));
            if (ProposalLease.IsHeldByOther(p, actor.EmployeeId, now))
                return new(actor, p, BudgetResponses.Fail(BudgetMessages.M23));
            return new(actor, p, null);
        }

        /// <summary>
        /// Takes the proposal row lock inside the action scope, so concurrent writes to one demand allot serials
        /// (max + 1, BUD-079) and evaluate VR-14 one after another.
        /// </summary>
        public static Task LockProposalAsync(BudgetContext context, long proposalId, CancellationToken ct) =>
            context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM budget.proposal WHERE proposal_id = {proposalId} FOR UPDATE", ct);

        /// <summary>Every write by the holder office renews the lease (design §4.10).</summary>
        public static void RenewLease(BudgetProposal p, BudgetActor actor, DateTimeOffset now, TimeSpan length)
        {
            p.EditLeaseBy = actor.EmployeeId;
            p.EditLeaseExpiresAt = now + length;
        }

        /// <summary>
        /// Runs the Save moment over the proposal's non-deleted lines with <paramref name="candidate"/> in its new
        /// form, and keeps only what concerns the candidate: a pre-existing incomplete line does not block the
        /// save of another (BUD-241). The candidate's flags are written onto it.
        /// </summary>
        public static async Task<DemandLineRuleResult> EvaluateSaveAsync(
            BudgetContext context, IDemandLineRules rules, BudgetProposal p, DemandLine candidate, string actorLevel, CancellationToken ct)
        {
            var others = await context.DemandLines.AsNoTracking()
                .Where(l => l.ProposalId == p.ProposalId && !l.IsDeleted && l.DemandLineId != candidate.DemandLineId)
                .ToListAsync(ct);
            var all = await rules.EvaluateAsync(p, [.. others, candidate], RuleMoment.Save, actorLevel, ct);
            long? id = candidate.DemandLineId > 0 ? candidate.DemandLineId : null;
            var mine = new DemandLineRuleResult();
            mine.Failures.AddRange(all.Failures.Where(f => f.DemandLineId == id));
            mine.Warnings.AddRange(all.Warnings.Where(f => f.DemandLineId == id));
            if (all.Flags.FirstOrDefault(f => f.DemandLineId == id) is { } flags)
            {
                candidate.PctCheckNotApplied = flags.PctCheckNotApplied;
                candidate.FloorCheckUnchecked = flags.FloorCheckUnchecked;
            }
            return mine;
        }

        /// <summary>A single field refusal in the rule-failure shape (design §4.9).</summary>
        public static Response<RuleFailuresDto> FieldFailure(DemandLine line, string field, string messageId) =>
            BudgetResponses.RuleFailures(new RuleFailuresDto
            {
                Failures =
                [
                    new RuleFailureItemDto(line.DemandLineId > 0 ? line.DemandLineId : null, line.SerialNo > 0 ? line.SerialNo : null,
                        field, messageId, BudgetMessages.Format(messageId)),
                ],
            });

        /// <summary>The T-07 code a work's six segments resolve to, or null (design §5.2, §5.5 row i).</summary>
        public static Task<long?> ResolvedAccountCodeIdAsync(BudgetContext context, Work w, CancellationToken ct) =>
            context.HeadOfAccounts.AsNoTracking()
                .Where(h => h.MajorAccCode == w.MajorAccCode && h.SubMajorAccCode == w.SubMajorAccCode &&
                            h.MinorAccCode == w.MinorAccCode && h.SubMinorAccCode == w.SubMinorAccCode &&
                            h.DetailAccCode == w.DetailAccCode && h.ObjectAccCode == w.ObjectAccCode)
                .Select(h => (long?)h.AccountCodeId).FirstOrDefaultAsync(ct);

        /// <summary>The master Division id of the proposal's origin office (WORKS proposals originate at a Division).</summary>
        public static Task<int?> OriginDivisionIdAsync(BudgetContext context, BudgetProposal p, CancellationToken ct) =>
            context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == p.OriginOfficeId).Select(o => o.DivisionId).SingleAsync(ct);

        /// <summary>
        /// AM&amp;R works of the same Division with a non-deleted WORK line in the WORKS proposal of the preceding
        /// cycle (design §8.3 amr-predecessor-options, BUD-275). Empty where no preceding cycle exists.
        /// </summary>
        public static async Task<IReadOnlyList<Work>> AmrPredecessorsAsync(BudgetContext context, BudgetProposal p, CancellationToken ct)
        {
            if (p.Category != ProposalCategory.Works)
                return [];
            var year = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == p.CycleId).Select(c => c.CurrentFinYear).SingleAsync(ct);
            var ids = await (
                from c in context.BudgetCycles.AsNoTracking()
                where c.CurrentFinYear == year - 1
                join pp in context.BudgetProposals.AsNoTracking() on c.CycleId equals pp.CycleId
                where pp.Category == ProposalCategory.Works && pp.OriginOfficeId == p.OriginOfficeId
                join l in context.DemandLines.AsNoTracking() on pp.ProposalId equals l.ProposalId
                where l.LineType == DemandLineType.Work && !l.IsDeleted && l.WorkId != null
                select l.WorkId!.Value).Distinct().ToListAsync(ct);
            return await context.Works.AsNoTracking()
                .Where(w => ids.Contains(w.Id) && w.WorkClass == WorkClass.Amr)
                .OrderBy(w => w.Id).ToListAsync(ct);
        }
    }
}
