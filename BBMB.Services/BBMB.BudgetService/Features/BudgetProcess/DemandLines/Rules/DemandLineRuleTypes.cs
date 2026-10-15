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
        // ---------------- P2 (P2-12): VR-03, VR-04, VR-05, VR-06, VR-10 ----------------
        /// <summary>The work's class (AA / AMR); null for HEAD and UNFORESEEN lines.</summary>
        public string? WorkClass { get; init; }
        /// <summary>A T-25 approval exists for the work (VR-03 applies only where none does).</summary>
        public bool WorkHasApproval { get; init; }
        /// <summary>The amount of the approval's current T-26 version (VR-04 base).</summary>
        public decimal? AaAmount { get; init; }
        /// <summary>Column 14: the AM&amp;R work this AM&amp;R work continues (BUD-275).</summary>
        public int? PrecedingAmrWorkId { get; init; }
        /// <summary>
        /// BUD-276 "latest budget approved" for the column-14 work: its revised estimate allocated, else its original
        /// estimate allocated. Null before P4 (no provision exists), when VR-05 is not applied and the justification is
        /// mandatory instead.
        /// </summary>
        public decimal? AmrApprovedBudget { get; init; }
        /// <summary>VR-06: the HEAD line's code is assigned to the origin office and not removed (DB-02).</summary>
        public bool HeadAssigned { get; init; } = true;
        public long? CeCertificateAttachmentId { get; init; }
        public long? CeAmrApprovalAttachmentId { get; init; }
        /// <summary>VR-10: each of the two CE attachments named on the line exists and is of evidence class (DB-08).</summary>
        public bool CeCertificateIsEvidence { get; init; } = true;
        public bool CeAmrApprovalIsEvidence { get; init; } = true;
    }
    /// <summary>The T-17 ceilings in force (design §6.4); a null value means no row in force and the ceiling is not applied.</summary>
    public sealed record DemandCeilings(decimal? NoAaLimitRupees, decimal? AaDemandCeilingPct, decimal? AmrIncreasePct);

    /// <summary>The proposal-level facts the evaluator needs.</summary>
    /// <param name="Ceilings">The T-17 ceilings in force; null (P1 callers) applies none of VR-03/04/05.</param>
    public sealed record DemandLineRuleContext(string Category, RuleMoment Moment, DemandCeilings? Ceilings = null);

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
