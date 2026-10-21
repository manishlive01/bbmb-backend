namespace BBMB.BudgetService.Features.BudgetProcess.Settings.SetExpenditureSource.Dtos
{
    /// <summary>Design §8.2 <c>POST /api/budget-settings/expenditure-source</c> body (BUD-025, BUD-312).</summary>
    public sealed class SetExpenditureSourceDto
    {
        public long OfficeId { get; set; }
        public int FinYear { get; set; }
        /// <summary>KEYED or DRAWN. DRAWN is refused while Financial Accounting is the not-available stub (BUD-312).</summary>
        public string Mode { get; set; } = string.Empty;
    }
}
