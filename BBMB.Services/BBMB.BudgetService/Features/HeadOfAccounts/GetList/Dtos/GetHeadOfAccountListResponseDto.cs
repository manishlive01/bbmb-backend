namespace BBMB.BudgetService.Features.HeadOfAccounts.GetList.Dtos
{
    public class GetHeadOfAccountListResponseDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorHeadDescription { get; set; } = string.Empty;
        public string? SubMajorAccCode { get; set; }
        public string? SubMajorHeadDescription { get; set; }
        public string MinorAccCode { get; set; } = string.Empty;
        public string MinorHeadDescription { get; set; } = string.Empty;
        public string? SubMinorAccCode { get; set; }
        public string? SubMinorHeadDescription { get; set; }
        public string DetailAccCode { get; set; } = string.Empty;
        public string DetailHeadDescription { get; set; } = string.Empty;
        public string ObjectAccCode { get; set; } = string.Empty;
        public string ObjectClassificationDescription { get; set; } = string.Empty;
        public string HeadDesc { get; set; } = string.Empty;
        public string FullAccountCode { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
