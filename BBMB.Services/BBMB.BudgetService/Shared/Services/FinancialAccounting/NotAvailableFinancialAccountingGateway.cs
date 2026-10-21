namespace BBMB.BudgetService.Shared.Services.FinancialAccounting
{
    /// <summary>Design §4.11: Financial Accounting is not integrated; every answer is Unavailable.</summary>
    public sealed class NotAvailableFinancialAccountingGateway : IFinancialAccountingGateway
    {
        public Task<FinancialAccountingResult> GetExpenditureAsync(
            FinancialAccountingSubject subject, int finYear, string period, CancellationToken ct = default) =>
            Task.FromResult(FinancialAccountingResult.Unavailable);

        public Task<FinancialAccountingResult> GetBookedAgainstEstimateCodeAsync(string estimateCode, CancellationToken ct = default) =>
            Task.FromResult(FinancialAccountingResult.Unavailable);
    }
}
