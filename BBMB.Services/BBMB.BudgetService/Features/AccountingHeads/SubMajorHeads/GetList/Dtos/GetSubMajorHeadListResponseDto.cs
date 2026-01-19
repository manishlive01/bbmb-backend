namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList.Dtos
{
    public class GetSubMajorHeadListResponseDto
    {
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccDesc { get; set; } = string.Empty;
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
