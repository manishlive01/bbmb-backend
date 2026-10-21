namespace BBMB.BudgetService.Features.HeadOfAccounts.ParseAccountCode.Dtos
{
    public class ParseAccountCodeResponseDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string? MajorHeadDescription { get; set; }
        public string SubMajorAccCode { get; set; } = string.Empty;
        public string? SubMajorHeadDescription { get; set; }
        public string MinorAccCode { get; set; } = string.Empty;
        public string? MinorHeadDescription { get; set; }
        public string SubMinorAccCode { get; set; } = string.Empty;
        public string? SubMinorHeadDescription { get; set; }
        public string DetailAccCode { get; set; } = string.Empty;
        public string? DetailHeadDescription { get; set; }
        public string ObjectAccCode { get; set; } = string.Empty;
        public string? ObjectClassificationDescription { get; set; }
        /// <summary>The T-07 code the six segments form, where it exists in the Manual; null otherwise.</summary>
        public long? AccountCodeId { get; set; }
        public string? CompleteCode { get; set; }
        public bool? NotForUse { get; set; }
        public string? Origin { get; set; }
    }
}
