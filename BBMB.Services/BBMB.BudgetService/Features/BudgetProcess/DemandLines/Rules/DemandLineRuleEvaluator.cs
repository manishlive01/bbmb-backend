using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules
{
    /// <summary>
    /// The P1 rows of the design §6.4 line rule set, as a pure function of the loaded inputs (no I/O, so every
    /// boundary is unit-tested). Rows by moment:
    /// <list type="bullet">
    /// <item>VR-09 (M-05), VR-14 (M-25 work / M-35 head), pre-DDD work (M-21), the WORK line's code equal to its
    /// work's resolved code (design §5.5 row i, M-22): every moment.</item>
    /// <item>Completeness (M-21): Submit, Upload, ReviewerForward, FinanceFinalise.</item>
    /// <item>Justification where an observation exists (M-21): Submit, Upload, ReviewerForward.</item>
    /// <item>VR-15 floor (M-36): every moment, not RECEIPTS; skipped when rbe is null (BUD-241); expenditure
    /// unavailable → <c>floor_check_unchecked</c>, not a refusal.</item>
    /// <item>BUD-243: a nil or negative percentage base → <c>pct_check_not_applied</c> and an M-32 warning (not at
    /// FinanceFinalise).</item>
    /// <item>VR-03, VR-04, VR-05 (warn on Save, refuse on Submit, skipped by Finance) and VR-06 / VR-10 on the
    /// line's attachments (P2-12): see <see cref="CeilingRules"/> and <see cref="HeadAssignment"/>.</item>
    /// </list>
    /// </summary>
    public static class DemandLineRuleEvaluator
    {
        public static DemandLineRuleResult Evaluate(DemandLineRuleContext context, IReadOnlyList<DemandLineRuleInput> lines)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(lines);
            var result = new DemandLineRuleResult();
            var m = context.Moment;
            var completeness = m is RuleMoment.Submit or RuleMoment.Upload or RuleMoment.ReviewerForward or RuleMoment.FinanceFinalise;
            var justificationOnObservation = m is RuleMoment.Submit or RuleMoment.Upload or RuleMoment.ReviewerForward;

            // VR-14: one line per work (M-25, BUD-104) and per head in a head-wise demand (M-35, BUD-310).
            var duplicateWorks = lines.Where(l => l.LineType == DemandLineType.Work && l.WorkId is not null)
                .GroupBy(l => l.WorkId).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();
            var duplicateHeads = lines.Where(l => l.LineType == DemandLineType.Head && l.AccountCodeId is not null)
                .GroupBy(l => l.AccountCodeId).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

            foreach (var line in lines)
            {
                void Fail(string field, string id) => result.Failures.Add(Item(line, field, id));

                // VR-09 references.
                if (line.LineType != DemandLineType.Unforeseen && !line.AccountCodeUsable)
                    Fail("accountCodeId", BudgetMessages.M05);
                if (line.LineType == DemandLineType.Work)
                {
                    if (!line.WorkExists)
                        Fail("workId", BudgetMessages.M05);
                    else if (line.WorkPreDdd)
                        Fail("workId", BudgetMessages.M21); // work particulars incomplete (design §5.2 row a)
                    else if (line.AccountCodeId != line.WorkAccountCodeId)
                        Fail("accountCodeId", BudgetMessages.M22); // design §5.5 row i (no DB rule covers it)
                    if (duplicateWorks.Contains(line.WorkId))
                        Fail("workId", BudgetMessages.M25);
                }
                if (line.LineType == DemandLineType.Head && duplicateHeads.Contains(line.AccountCodeId))
                    Fail("accountCodeId", BudgetMessages.M35);

                // Completeness (BUD-010).
                if (completeness)
                {
                    if (line.RbeAmount is null)
                        Fail("rbeAmount", BudgetMessages.M21);
                    if (line.ObeNextAmount is null)
                        Fail("obeNextAmount", BudgetMessages.M21);
                    if (line.LineType == DemandLineType.Unforeseen && string.IsNullOrWhiteSpace(line.UnforeseenName))
                        Fail("unforeseenName", BudgetMessages.M21);
                }
                if (justificationOnObservation && line.HasObservation && string.IsNullOrWhiteSpace(line.JustificationText))
                    Fail("justificationText", BudgetMessages.M21);

                // VR-15 floor (BUD-317): not lifted at any level; not for receipts (BUD-318).
                var floorUnchecked = false;
                if (context.Category != ProposalCategory.Receipts && line.RbeAmount is { } rbe)
                {
                    if (line.ExpenditureUpToDate is { } spent)
                    {
                        if (rbe < spent)
                            Fail("rbeAmount", BudgetMessages.M36);
                    }
                    else
                        floorUnchecked = true;
                }

                // BUD-243: no percentage is computed against a nil or negative base.
                var pctNotApplied = false;
                if (m != RuleMoment.FinanceFinalise && line.PercentageBase is { } basis && basis <= 0)
                {
                    pctNotApplied = true;
                    result.Warnings.Add(Item(line, "rbeAmount", BudgetMessages.M32));
                }

                CeilingRules(context, line, result); // VR-03/04/05, P2
                HeadAssignment(context, line, result); // VR-06, P2

                result.Flags.Add(new DemandLineFlags(line.DemandLineId, line.SerialNo, pctNotApplied, floorUnchecked));
            }
            return result;
        }

        /// <summary>
        /// VR-03 / VR-04 / VR-05 (design §6.4, OI-24): evaluated on the absolute value of each figure (BUD-089); a warning
        /// at Save and reviewer alteration, a refusal at Submit, Upload and reviewer Forward, not applied by Finance
        /// (BUD-126). A null figure is a part entry and is not measured (BUD-241).
        /// <list type="bullet">
        /// <item>VR-03 (BUD-090, BUD-091; M-08): an AA-tab work with no approval, each column above NO_AA_LIMIT_RUPEES,
        /// unless the CE's certificate is attached; with the certificate the justification becomes mandatory (M-21).</item>
        /// <item>VR-04 (BUD-092; M-09): an AA-tab work with an approval, |rbe| + |obe| above AA_DEMAND_CEILING_PCT of
        /// the current approval amount (no override; the technical sanction total does not limit it).</item>
        /// <item>VR-05 (BUD-093, BUD-276, BUD-278; M-10): an AM&amp;R work, each column above (100 + AMR_INCREASE_PCT) %
        /// of the latest budget approved for its column-14 work, unless the justification and the CE's AM&amp;R approval
        /// are both attached. Where column 14 is empty or that work carries no approved budget (always, before P4), VR-05
        /// is not applied and the justification is mandatory instead (M-21 at Submit-type moments).</item>
        /// </list>
        /// Comparisons are × 100 against the percentage, so a figure exactly at the ceiling passes.
        /// </summary>
        private static void CeilingRules(DemandLineRuleContext context, DemandLineRuleInput line, DemandLineRuleResult result)
        {
            var c = context.Ceilings;
            if (c is null || line.LineType != DemandLineType.Work)
                return;
            var m = context.Moment;
            if (m is RuleMoment.FinanceAlter or RuleMoment.FinanceFinalise)
                return;
            var refuse = m is RuleMoment.Submit or RuleMoment.Upload or RuleMoment.ReviewerForward;
            void Ceiling(string field, string id)
            {
                if (refuse) result.Failures.Add(Item(line, field, id));
                else result.Warnings.Add(Item(line, field, id));
            }
            void JustificationRequired()
            {
                if (refuse && string.IsNullOrWhiteSpace(line.JustificationText))
                    result.Failures.Add(Item(line, "justificationText", BudgetMessages.M21));
            }
            var columns = new (string Field, decimal? Value)[] { ("rbeAmount", line.RbeAmount), ("obeNextAmount", line.ObeNextAmount) };
            if (line.WorkClass == Works.Entities.WorkClass.Aa && !line.WorkHasApproval)
            {
                if (c.NoAaLimitRupees is not { } limit)
                    return;
                var crossed = columns.Where(x => x.Value is { } v && Math.Abs(v) > limit).ToList();
                if (crossed.Count == 0)
                    return;
                if (line.CeCertificateAttachmentId is null)
                    foreach (var (field, _) in crossed) Ceiling(field, BudgetMessages.M08);
                else
                    JustificationRequired();
            }
            else if (line.WorkClass == Works.Entities.WorkClass.Aa)
            {
                if (c.AaDemandCeilingPct is not { } pct || line.AaAmount is not { } aa || aa <= 0)
                    return; // a nil or negative base is BUD-243 (flagged above), not a ceiling
                var sum = Math.Abs(line.RbeAmount ?? 0) + Math.Abs(line.ObeNextAmount ?? 0);
                if (sum * 100 > pct * aa)
                    Ceiling("obeNextAmount", BudgetMessages.M09);
            }
            else if (line.WorkClass == Works.Entities.WorkClass.Amr)
            {
                if (line.PrecedingAmrWorkId is null || line.AmrApprovedBudget is not { } budget)
                {
                    JustificationRequired(); // BUD-276 "neither" branch
                    return;
                }
                if (c.AmrIncreasePct is not { } pct || budget <= 0)
                    return;
                var crossed = columns.Where(x => x.Value is { } v && Math.Abs(v) * 100 > (100 + pct) * budget).ToList();
                if (crossed.Count == 0)
                    return;
                if (string.IsNullOrWhiteSpace(line.JustificationText) || line.CeAmrApprovalAttachmentId is null)
                    foreach (var (field, _) in crossed) Ceiling(field, BudgetMessages.M10);
            }
        }
        /// <summary>
        /// VR-06 (BUD-136; M-13; DB-02): a HEAD line only for a head assigned to the origin office and not removed, at every
        /// moment. VR-10 (DB-08): a CE certificate or CE AM&amp;R approval named on a line is of evidence class (M-24).
        /// </summary>
        private static void HeadAssignment(DemandLineRuleContext context, DemandLineRuleInput line, DemandLineRuleResult result)
        {
            if (line.LineType == DemandLineType.Head && line.AccountCodeId is not null && !line.HeadAssigned)
                result.Failures.Add(Item(line, "accountCodeId", BudgetMessages.M13));
            if (line.CeCertificateAttachmentId is not null && !line.CeCertificateIsEvidence)
                result.Failures.Add(Item(line, "ceCertificateAttachmentId", BudgetMessages.M24));
            if (line.CeAmrApprovalAttachmentId is not null && !line.CeAmrApprovalIsEvidence)
                result.Failures.Add(Item(line, "ceAmrApprovalAttachmentId", BudgetMessages.M24));
        }
        private static RuleFailureItemDto Item(DemandLineRuleInput line, string field, string messageId) =>
            new(line.DemandLineId, line.SerialNo, field, messageId, BudgetMessages.Format(messageId));
    }
}
