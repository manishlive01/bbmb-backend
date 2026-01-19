namespace BBMB.BudgetService.Features.DetailHeads.GetList.Dtos
{
    public class GetDetailHeadListResponseDto
    {
        public string DetailAccCode { get; set; } = string.Empty;
        public string DetailAccDesc { get; set; } = string.Empty;
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccDesc { get; set; } = string.Empty;
        public string MinorAccCode { get; set; } = string.Empty;
        public string MinorAccDesc { get; set; } = string.Empty;
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccDesc { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
    }
}
