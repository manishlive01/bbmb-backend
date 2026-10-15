namespace BBMB.BudgetService.Features.Works.Update.Dtos
{
    public class UpdateWorkDto
    {
        // P4 (design §4.6): UpdateWorkDto loses WorkCode. A work code is never set or changed through the
        // work master update — it is generated only on allocation (BUD-232) or keyed via F-28 legacy-code,
        // and is write-once (DB-05).
        public string WorkDesc { get; set; } = string.Empty;
        // No DivisionId: division_id is never updatable (design §11.3). No enteredVia (design §9.3).
        public int DivSectionId { get; set; }
        /// <summary>
        /// T-07 surrogate key; null keeps the current head. A change is refused with M-19 once the work is
        /// referenced (design §11.3), and is resolved to the six segments otherwise (design §5.2).
        /// </summary>
        public long? AccountCodeId { get; set; }
        /// <summary>AA or AMR; null keeps the current class. Required to clear <c>pre_ddd</c> on an old row.</summary>
        public string? WorkClass { get; set; }
        /// <summary>AM&amp;R predecessor; null keeps the current value.</summary>
        public int? PrecedingAmrWorkId { get; set; }
        public string UserCode { get; set; } = "123";
        public string? FinYrStart { get; set; }
        public string? FinYrEnd { get; set; }
        public string? SanctionNo { get; set; }
        public string? SanctionDate { get; set; }
        public string? DesignationCode { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Status { get; set; }
        public string? Nature { get; set; }
        public string? RefWorkCode { get; set; }
        public int CECode { get; set; }
        public int CRCode { get; set; }
        public string OriginalBudget { get; set; } = string.Empty;
        public string RevisedBudget { get; set; } = string.Empty;
        public string ProposedBudget { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
