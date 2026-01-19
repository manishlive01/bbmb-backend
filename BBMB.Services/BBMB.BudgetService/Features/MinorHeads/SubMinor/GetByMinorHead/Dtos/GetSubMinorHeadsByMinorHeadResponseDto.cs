namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.GetByMinorHead.Dtos
{
    public class GetSubMinorHeadsByMinorHeadResponseDto
    {
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string SubMinorAccDesc { get; set; } = string.Empty;
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string SubMajorAccDesc { get; set; } = string.Empty;
        public string MinorAccCode { get; set; } = string.Empty;
        public string MinorAccDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
