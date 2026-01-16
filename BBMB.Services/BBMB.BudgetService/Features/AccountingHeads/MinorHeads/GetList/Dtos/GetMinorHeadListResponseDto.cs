namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList.Dtos
{
    public class GetMinorHeadListResponseDto
    {
        public string MinorAccCode { get; set; } = string.Empty;
        public string MinorAccDesc { get; set; } = string.Empty;
        public string ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public int SubMinorCount { get; set; }
    }
}
