namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetList.Dtos
{
    public class GetMajorHeadListResponseDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
        public string ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public int SubMajorCount { get; set; }
    }
}
