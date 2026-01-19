namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById.Dtos
{
    public class GetSubMajorHeadByIdResponseDto
    {
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccDesc { get; set; } = string.Empty;
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
