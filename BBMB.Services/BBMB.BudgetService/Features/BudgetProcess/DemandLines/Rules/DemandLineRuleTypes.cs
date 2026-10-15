using BBMB.BudgetService.Shared.ResponseDTOs;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules
{
    /// <summary>When the line rule set runs (design §6.4).</summary>
    public enum RuleMoment
    {
        Save,
        Submit,
        Upload,
        ReviewerAlter,
        ReviewerForward,
        FinanceAlter,
        FinanceFinalise,
    }

    /// <summary>
    /// One line as the evaluator sees it: the line's own fields and the facts it is checked against, all loaded
    /// beforehand by <see cref="DemandLineRuleInputsLoader"/> so the evaluator is pure.
    /// </summary>
    public sealed record DemandLineRuleInput
    {
        /// <summary>Null for a line not yet saved (Add, Upload).</summary>
        public long? DemandLineId { get; init; }
        public int? SerialNo { get; init; }
        public required string LineType { get; init; }
        public int? WorkId { get; init; }
        public long? AccountCodeId { get; init; }
        public string? UnforeseenName { get; init; }
        public decimal? RbeAmount { get; init; }
        public decimal? ObeNextAmount { get; init; }
        public string? JustificationText { get; init; }

        /// <summary>The line's account code exists in T-07 and is not marked not for use (VR-09).</summary>
        public bool AccountCodeUsable { get; init; } = true;
        /// <summary>The work exists (WORK lines).</summary>
        public bool WorkExists { get; init; } = true;
        /// <summary>The work is an old row not yet classified (design §5.2 row a): refused M-21.</summary>
        public bool WorkPreDdd { get; init; }
        /// <summary>The T-07 code the work's six segments resolve to; null when they resolve to none.</summary>
        public long? WorkAccountCodeId { get; init; }
        /// <summary>An observation exists on the line (justification then mandatory on Submit).</summary>
        public bool HasObservation { get; init; }
        /// <summary>
        /// Expenditure up to date of the current year (Table 3 col 10 / Table 4 col 6); null when unavailable
        /// (VR-15 then flags the line instead of refusing it).
        /// </summary>
        public decimal? ExpenditureUpToDate { get; init; }
        /// <summary>
        /// The base a percentage ceiling is measured against (VR-04 AA amount, VR-05 approved budget), or null
        /// where no percentage ceiling applies in the deployed phase. Nil or negative → BUD-243 flag.
        /// </summary>
        public decimal? PercentageBase { get; init; }
    }

    /// <summary>The proposal-level facts the evaluator needs.</summary>
    public sealed record DemandLineRuleContext(string Category, RuleMoment Moment);

    /// <summary>The flags a Save writes onto the line (DDD T-30).</summary>
    public sealed record DemandLineFlags(long? DemandLineId, int? SerialNo, bool PctCheckNotApplied, bool FloorCheckUnchecked);

    /// <summary>Result of one evaluation: refusals block, warnings are returned with the save (design §6.4).</summary>
    public sealed class DemandLineRuleResult
    {
        public List<RuleFailureItemDto> Failures { get; } = [];
        public List<RuleFailureItemDto> Warnings { get; } = [];
        public List<DemandLineFlags> Flags { get; } = [];
        public bool Refused => Failures.Count > 0;

        public RuleFailuresDto ToDto() => new() { Failures = [.. Failures], Warnings = [.. Warnings] };
    }
}
