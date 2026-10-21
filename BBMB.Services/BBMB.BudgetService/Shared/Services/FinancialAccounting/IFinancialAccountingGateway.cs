namespace BBMB.BudgetService.Shared.Services.FinancialAccounting
{
    /// <summary>The subject an expenditure figure is asked for: a work, or a head of an office (T-35 subject columns).</summary>
    public sealed record FinancialAccountingSubject(string SubjectType, int? WorkId, long? OfficeId, long? AccountCodeId);

    /// <summary>A value from Financial Accounting, or <see cref="Unavailable"/> when no source answers.</summary>
    public sealed record FinancialAccountingResult(bool Available, decimal? Amount)
    {
        public static readonly FinancialAccountingResult Unavailable = new(false, null);
    }

    /// <summary>
    /// Design §4.11 boundary to Financial Accounting (INT-002). No integration exists in this scope: the only
    /// implementation is <see cref="NotAvailableFinancialAccountingGateway"/>, so expenditure runs in KEYED mode
    /// and VR-11 is not applied (OI-15). No call leaves the service.
    /// </summary>
    public interface IFinancialAccountingGateway
    {
        /// <param name="period">FULL_YEAR, UP_TO_MONTH or UP_TO_DATE (T-35 <c>period</c>).</param>
        Task<FinancialAccountingResult> GetExpenditureAsync(
            FinancialAccountingSubject subject, int finYear, string period, CancellationToken ct = default);

        /// <summary>Expenditure booked against an estimate code (VR-11, BUD-273).</summary>
        Task<FinancialAccountingResult> GetBookedAgainstEstimateCodeAsync(string estimateCode, CancellationToken ct = default);
    }
}
