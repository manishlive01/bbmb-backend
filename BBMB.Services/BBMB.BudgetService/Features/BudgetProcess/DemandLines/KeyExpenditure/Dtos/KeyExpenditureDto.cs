namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure.Dtos
{
    /// <summary>Design §8.3 <c>POST …/demand-lines/{lineId}/expenditure</c> body (T-35).</summary>
    public sealed class KeyExpenditureDto
    {
        /// <summary>EXPENDITURE, or RECEIPT for a RECEIPTS proposal (BUD-161, BUD-162).</summary>
        public string Nature { get; set; } = string.Empty;
        /// <summary>FULL_YEAR (the previous year) or UP_TO_MONTH / UP_TO_DATE (the current year).</summary>
        public string Period { get; set; } = string.Empty;
        /// <summary>First day of the month; exactly for UP_TO_MONTH (BUD-141).</summary>
        public DateOnly? UpToMonth { get; set; }
        /// <summary>Whole rupees where keyed (BUD-013).</summary>
        public decimal Amount { get; set; }
    }
}
