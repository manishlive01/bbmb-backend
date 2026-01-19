namespace BBMB.BudgetService.Features.HeadOfAccounts.GetById.Dtos
{
    public class GetHeadOfAccountByIdResponseDto
    {
        public int Id { get; set; }
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
        public string? PartyEmpRequired { get; set; }
        public string? PartyLedgToPrint { get; set; }
        public string? TdsDeducted { get; set; }
        public string? AdvanceHead { get; set; }
        public string? BankCash { get; set; }
        public string? MonthlyCarryFwd { get; set; }
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
