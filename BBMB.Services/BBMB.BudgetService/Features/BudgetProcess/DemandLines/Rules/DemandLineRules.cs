using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules
{
    /// <summary>The line rule set (design §6.4): one method, failures and warnings per line.</summary>
    public interface IDemandLineRules
    {
        /// <param name="lines">
        /// The non-deleted lines the moment covers: for Save the proposal's lines with the candidate line in its
        /// new form (VR-14 compares them); for Submit-type moments every non-deleted line.
        /// </param>
        /// <param name="actorLevel">The acting office's level (P2+ rules differ by level; unused in P1).</param>
        Task<DemandLineRuleResult> EvaluateAsync(
            BudgetProposal proposal, IReadOnlyList<DemandLine> lines, RuleMoment moment, string actorLevel, CancellationToken ct = default);
    }

    public sealed class DemandLineRules(DemandLineRuleInputsLoader loader) : IDemandLineRules
    {
        public async Task<DemandLineRuleResult> EvaluateAsync(
            BudgetProposal proposal, IReadOnlyList<DemandLine> lines, RuleMoment moment, string actorLevel, CancellationToken ct = default)
        {
            var inputs = await loader.LoadAsync(proposal, lines, ct);
            var ceilings = await loader.LoadCeilingsAsync(ct);
            return DemandLineRuleEvaluator.Evaluate(new DemandLineRuleContext(proposal.Category, moment, ceilings), inputs);
        }
    }

    /// <summary>
    /// Loads everything the pure evaluator needs in a fixed number of queries: account-code usability (VR-09),
    /// each work's state and resolved code, observations per line, and the expenditure up to date of the current
    /// year (latest UP_TO_DATE or UP_TO_MONTH EXPENDITURE row of T-35; none → unavailable).
    /// </summary>
    public sealed class DemandLineRuleInputsLoader(BudgetContext context, TimeProvider? clock = null)
    {
        /// <summary>The three T-17 ceilings in force now (design §6.4); a key with no row in force is not applied.</summary>
        public async Task<DemandCeilings> LoadCeilingsAsync(CancellationToken ct)
        {
            var reader = new Settings.BudgetSettingsReader(context);
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            return new DemandCeilings(
                (await reader.GetAsync(Settings.Entities.SettingKey.NoAaLimitRupees, now, ct))?.NumericValue,
                (await reader.GetAsync(Settings.Entities.SettingKey.AaDemandCeilingPct, now, ct))?.NumericValue,
                (await reader.GetAsync(Settings.Entities.SettingKey.AmrIncreasePct, now, ct))?.NumericValue);
        }
        private static readonly string[] UpToDatePeriods = ["UP_TO_DATE", "UP_TO_MONTH"];

        public async Task<IReadOnlyList<DemandLineRuleInput>> LoadAsync(BudgetProposal proposal, IReadOnlyList<DemandLine> lines, CancellationToken ct)
        {
            var finYear = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == proposal.CycleId).Select(c => c.CurrentFinYear).SingleAsync(ct);

            var codeIds = lines.Where(l => l.AccountCodeId is not null).Select(l => l.AccountCodeId!.Value).Distinct().ToList();
            var usable = (await context.HeadOfAccounts.AsNoTracking()
                .Where(h => codeIds.Contains(h.AccountCodeId) && !h.NotForUse).Select(h => h.AccountCodeId).ToListAsync(ct)).ToHashSet();

            var workIds = lines.Where(l => l.WorkId is not null).Select(l => l.WorkId!.Value).Distinct().ToList();
            var works = await (
                from w in context.Works.AsNoTracking()
                where workIds.Contains(w.Id)
                join h in context.HeadOfAccounts.AsNoTracking()
                    on new { w.MajorAccCode, w.SubMajorAccCode, w.MinorAccCode, w.SubMinorAccCode, w.DetailAccCode, w.ObjectAccCode }
                    equals new { h.MajorAccCode, SubMajorAccCode = h.SubMajorAccCode!, h.MinorAccCode, SubMinorAccCode = h.SubMinorAccCode!, h.DetailAccCode, h.ObjectAccCode }
                    into hj
                from h in hj.DefaultIfEmpty()
                select new { w.Id, w.PreDdd, AccountCodeId = (long?)h.AccountCodeId, w.WorkClass, w.PrecedingAmrWorkId }).ToListAsync(ct);
            // VR-03 / VR-04: the approval of each work and the amount of its current version.
            var approvals = (await context.AdminApprovals.AsNoTracking()
                    .Where(a => workIds.Contains(a.WorkId))
                    .Join(context.AdminApprovalVersions, a => new { a.AdminApprovalId, V = a.CurrentVersionNo },
                        v => new { v.AdminApprovalId, V = v.VersionNo }, (a, v) => new { a.WorkId, v.AmountSanctioned })
                    .ToListAsync(ct))
                .GroupBy(x => x.WorkId).ToDictionary(g => g.Key, g => g.First().AmountSanctioned);
            // VR-06: heads assigned live to the origin office.
            var assigned = (await context.AccountHeadAssignments.AsNoTracking()
                    .Where(a => a.OfficeId == proposal.OriginOfficeId && a.RemovedAt == null && codeIds.Contains(a.AccountCodeId))
                    .Select(a => a.AccountCodeId).ToListAsync(ct)).ToHashSet();
            // VR-10: evidence class of the CE attachments named on the lines.
            var attachmentIds = lines.SelectMany(l => new[] { l.CeCertificateAttachmentId, l.CeAmrApprovalAttachmentId })
                .Where(id => id is not null).Select(id => id!.Value).Distinct().ToList();
            var evidence = (await context.BudgetAttachments.AsNoTracking()
                    .Where(a => attachmentIds.Contains(a.AttachmentId) && a.EvidenceClass == Attachments.Entities.AttachmentEvidenceClass.Evidence)
                    .Select(a => a.AttachmentId).ToListAsync(ct)).ToHashSet();
            var workById = works.GroupBy(w => w.Id).ToDictionary(g => g.Key, g => g.First());

            var lineIds = lines.Where(l => l.DemandLineId > 0).Select(l => l.DemandLineId).ToList();
            var observed = (await context.Observations.AsNoTracking()
                .Where(o => o.DemandLineId != null && lineIds.Contains(o.DemandLineId.Value))
                .Select(o => o.DemandLineId!.Value).Distinct().ToListAsync(ct)).ToHashSet();

            var spent = await context.ExpenditureFigures.AsNoTracking()
                .Where(e => e.FinYear == finYear && e.Nature == "EXPENDITURE" && UpToDatePeriods.Contains(e.Period) &&
                            ((e.SubjectType == "WORK" && e.WorkId != null && workIds.Contains(e.WorkId.Value)) ||
                             (e.SubjectType == "HEAD" && e.OfficeId == proposal.OriginOfficeId && e.AccountCodeId != null && codeIds.Contains(e.AccountCodeId.Value))))
                .Select(e => new { e.SubjectType, e.WorkId, e.AccountCodeId, e.Amount, e.AsAt, e.ExpenditureFigureId })
                .ToListAsync(ct);
            decimal? SpentFor(DemandLine l) => l.LineType switch
            {
                DemandLineType.Work => spent.Where(e => e.SubjectType == "WORK" && e.WorkId == l.WorkId)
                    .OrderByDescending(e => e.AsAt).ThenByDescending(e => e.ExpenditureFigureId).Select(e => (decimal?)e.Amount).FirstOrDefault(),
                DemandLineType.Head => spent.Where(e => e.SubjectType == "HEAD" && e.AccountCodeId == l.AccountCodeId)
                    .OrderByDescending(e => e.AsAt).ThenByDescending(e => e.ExpenditureFigureId).Select(e => (decimal?)e.Amount).FirstOrDefault(),
                _ => null,
            };

            return lines.Select(l =>
            {
                var w = l.WorkId is { } id ? workById.GetValueOrDefault(id) : null;
                return new DemandLineRuleInput
                {
                    DemandLineId = l.DemandLineId > 0 ? l.DemandLineId : null,
                    SerialNo = l.SerialNo > 0 ? l.SerialNo : null,
                    LineType = l.LineType,
                    WorkId = l.WorkId,
                    AccountCodeId = l.AccountCodeId,
                    UnforeseenName = l.UnforeseenName,
                    RbeAmount = l.RbeAmount,
                    ObeNextAmount = l.ObeNextAmount,
                    JustificationText = l.JustificationText,
                    AccountCodeUsable = l.AccountCodeId is null || usable.Contains(l.AccountCodeId.Value),
                    WorkExists = l.WorkId is null || w is not null,
                    WorkPreDdd = w?.PreDdd ?? false,
                    WorkAccountCodeId = w?.AccountCodeId,
                    HasObservation = l.DemandLineId > 0 && observed.Contains(l.DemandLineId),
                    ExpenditureUpToDate = SpentFor(l),
                    WorkClass = w?.WorkClass,
                    WorkHasApproval = l.WorkId is { } aw && approvals.ContainsKey(aw),
                    AaAmount = l.WorkId is { } bw && approvals.TryGetValue(bw, out var amount) ? amount : null,
                    PrecedingAmrWorkId = w?.PrecedingAmrWorkId,
                    AmrApprovedBudget = null, // the latest approved budget comes from T-41 provisions (P4); BUD-276 "neither" until then
                    HeadAssigned = l.LineType != DemandLineType.Head || l.AccountCodeId is null || assigned.Contains(l.AccountCodeId.Value),
                    CeCertificateAttachmentId = l.CeCertificateAttachmentId,
                    CeAmrApprovalAttachmentId = l.CeAmrApprovalAttachmentId,
                    CeCertificateIsEvidence = l.CeCertificateAttachmentId is not { } cc || evidence.Contains(cc),
                    CeAmrApprovalIsEvidence = l.CeAmrApprovalAttachmentId is not { } ca || evidence.Contains(ca),
                    // The base of a percentage ceiling in force for the line (BUD-243 flags a nil or negative one):
                    // VR-04 the approval's current amount; VR-05 the approved budget (P4).
                    PercentageBase = l.WorkId is { } pw && approvals.TryGetValue(pw, out var basis) && w?.WorkClass == Works.Entities.WorkClass.Aa ? basis : null,
                };
            }).ToList();
        }
    }
}
