namespace BBMB.BudgetService.Features.HeadOfAccounts.Update.Dtos
{
    public class UpdateHeadOfAccountDto
    {
        public string HeadDesc { get; set; } = string.Empty;
        public string? PartyEmpRequired { get; set; }
        public string? PartyLedgToPrint { get; set; }
        public string? TdsDeducted { get; set; }
        public string? AdvanceHead { get; set; }
        public string? BankCash { get; set; }
        public string? MonthlyCarryFwd { get; set; }
        public string UserCode { get; set; } = "123";
    }
}
