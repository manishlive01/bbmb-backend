namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList.Dtos
{
    public class GetSubMajorHeadListResponseDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccDesc { get; set; } = string.Empty;
        public string ModificationDate { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
    }
}
