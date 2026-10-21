namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetExpenditureSource.Dtos
{
    /// <summary>Design §8.2: the expenditure-source mode in force for an office and year (KEYED where no row exists).</summary>
    public sealed record ExpenditureSourceDto(long OfficeId, int FinYear, string Mode);
}
