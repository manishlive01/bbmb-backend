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
    /// <item>VR-03, VR-04, VR-05 (warn on Save, refuse on Submit) and VR-06 arrive in P2 (P2-12); their hooks
    /// return nothing here.</item>
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

        /// <summary>VR-03 / VR-04 / VR-05 (design §6.4): implemented in P2-12; nothing in P1.</summary>
        private static void CeilingRules(DemandLineRuleContext context, DemandLineRuleInput line, DemandLineRuleResult result)
        {
        }

        /// <summary>VR-06 head assigned to the origin office (DB-02): implemented in P2-12; nothing in P1.</summary>
        private static void HeadAssignment(DemandLineRuleContext context, DemandLineRuleInput line, DemandLineRuleResult result)
        {
        }

        private static RuleFailureItemDto Item(DemandLineRuleInput line, string field, string messageId) =>
            new(line.DemandLineId, line.SerialNo, field, messageId, BudgetMessages.Format(messageId));
    }
}
