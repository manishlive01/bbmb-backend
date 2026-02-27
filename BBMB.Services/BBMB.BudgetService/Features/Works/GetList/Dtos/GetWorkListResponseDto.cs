namespace BBMB.BudgetService.Features.Works.GetList.Dtos
{
    //public class GetWorkListResponseDto
    //{
    //    public string WorkCode { get; set; } = string.Empty;
    //    public string WorkDesc { get; set; } = string.Empty;
    //    public int DivisionCode { get; set; }
    //    public string? DivisionDesc { get; set; }
    //    public string AccountCode { get; set; } = string.Empty;
    //    public string? AccountDescription { get; set; }
    //    public string? Status { get; set; }
    //    public string? Nature { get; set; }
    //    public string UserCode { get; set; } = string.Empty;
    //}
    public class GetWorkListResponseDto
    {
        public string WorkCode { get; set; } = string.Empty;

        public int DivisionCode { get; set; }
        public string? DivisionDesc { get; set; }

        public int DivSection { get; set; }

        public string MajorAccCode { get; set; } = string.Empty;
        public string? MajorAccDesc { get; set; }

        public string SubMajorAccCode { get; set; } = string.Empty;
        public string? SubMajorAccDesc { get; set; }

        public string MinorAccCode { get; set; } = string.Empty;
        public string? MinorAccDesc { get; set; }

        public string SubMinorAccCode { get; set; } = string.Empty;
        public string? SubMinorAccDesc { get; set; }

        public string DetailAccCode { get; set; } = string.Empty;
        public string? DetailAccDesc { get; set; }

        public string ObjectAccCode { get; set; } = string.Empty;
        public string? ObjectAccDesc { get; set; }

        public string? FinYrStart { get; set; }
        public string? FinYrEnd { get; set; }

        public string? SanctionNo { get; set; }
        public string? SanctionDate { get; set; }

        public string? DesignationCode { get; set; }
        public decimal? EstimatedCost { get; set; }

        public string WorkDesc { get; set; } = string.Empty;

        public string? Status { get; set; }
        public string? Nature { get; set; }

        public string? RefWorkCode { get; set; }

        public string UserCode { get; set; } = string.Empty;

        public string AccountCode { get; set; } = string.Empty;
        public string CECode { get; set; }
        public string CEName { get; set; } = string.Empty;
        public string CRCode { get; set; }
        public string CRName { get; set; } = string.Empty;
        public string OriginalBudget { get; set; } = string.Empty;
        public string RevisedBudget { get; set; } = string.Empty;
        public string ProposedBudget { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }

}
