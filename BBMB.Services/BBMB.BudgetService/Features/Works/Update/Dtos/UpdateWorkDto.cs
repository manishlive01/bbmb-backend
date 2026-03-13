namespace BBMB.BudgetService.Features.Works.Update.Dtos
{
    public class UpdateWorkDto
    {
        public string WorkCode { get; set; } = string.Empty;
        public string WorkDesc { get; set; } = string.Empty;
        public int DivisionCode { get; set; }
        public int DivSection { get; set; }
        public string MajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string MinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string DetailAccCode { get; set; } = string.Empty;
        public string ObjectAccCode { get; set; } = string.Empty;
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
