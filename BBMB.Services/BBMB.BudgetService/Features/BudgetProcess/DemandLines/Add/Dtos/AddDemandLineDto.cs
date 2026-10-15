namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos
{
    /// <summary>Design §8.3 <c>POST /api/budget-proposals/{id}/demand-lines</c> body.</summary>
    public sealed class AddDemandLineDto
    {
        /// <summary>WORK or UNFORESEEN in a WORKS proposal; HEAD in a head-wise one (DB-01).</summary>
        public string LineType { get; set; } = string.Empty;
        /// <summary>An existing work of the proposal's Division (WORK; exactly one of workId / newWork).</summary>
        public int? WorkId { get; set; }
        /// <summary>A work entered with the demand (WORK): created with <c>entered_via = DEMAND_ENTRY</c>.</summary>
        public NewWorkDto? NewWork { get; set; }
        /// <summary>HEAD: the head demanded. WORK: optional, must equal the work's code (design §5.5 row i).</summary>
        public long? AccountCodeId { get; set; }
        /// <summary>UNFORESEEN: the name of the unforeseen work (≤ 500).</summary>
        public string? UnforeseenName { get; set; }
        /// <summary>Revised estimate of the current year, whole rupees; negatives allowed (BUD-088).</summary>
        public decimal? RbeAmount { get; set; }
        /// <summary>Original estimate of the next year, whole rupees; negatives allowed (BUD-088).</summary>
        public decimal? ObeNextAmount { get; set; }
        public string? JustificationText { get; set; }
        /// <summary>Table 3 column 14: the AM&amp;R work of the preceding year this AM&amp;R work continues (BUD-275).</summary>
        public int? PrecedingAmrWorkId { get; set; }
    }

    /// <summary>A work entered with its demand line (design §8.3 <c>newWork</c>).</summary>
    public sealed class NewWorkDto
    {
        public string NameOfWork { get; set; } = string.Empty;
        /// <summary>T-07 code; the work takes its six segments (design §5.2).</summary>
        public long AccountCodeId { get; set; }
        /// <summary>AA or AMR.</summary>
        public string WorkClass { get; set; } = string.Empty;
    }
}
