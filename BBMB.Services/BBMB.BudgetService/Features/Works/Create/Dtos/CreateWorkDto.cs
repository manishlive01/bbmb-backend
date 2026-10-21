namespace BBMB.BudgetService.Features.Works.Create.Dtos
{
    public class CreateWorkDto
    {
        public string WorkCode { get; set; } = string.Empty;
        public string WorkDesc { get; set; } = string.Empty;

        /// <summary>AA or AMR (DDD T-23 work_class; required for every new work, design §9.3).</summary>
        public string WorkClass { get; set; } = string.Empty;
        /// <summary>Must be the acting DDO's Division (design §11.3 step 1), else M-20.</summary>
        public int DivisionId { get; set; }
        public int DivSectionId { get; set; }
        /// <summary>
        /// T-07 surrogate key. The handler resolves it to the six segments and copies them onto the work
        /// (design §5.2); the segments are not taken from the client.
        /// </summary>
        public long AccountCodeId { get; set; }
        /// <summary>AM&amp;R predecessor (AMR only; same Division, preceding year, DB-04).</summary>
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
        public string CECode { get; set; }
        public string CRCode { get; set; }
        public string OriginalBudget { get; set; } = string.Empty;
        public string RevisedBudget { get; set; } = string.Empty;
        public string ProposedBudget { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
