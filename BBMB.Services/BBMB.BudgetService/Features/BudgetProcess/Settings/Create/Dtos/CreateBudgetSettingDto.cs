namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Create.Dtos
{
    /// <summary>Body of <c>POST /api/budget-settings</c>: a new T-17 row (append-only, design §8.2).</summary>
    public sealed class CreateBudgetSettingDto
    {
        public string SettingKey { get; set; } = string.Empty;
        public decimal? NumericValue { get; set; }
        /// <summary>UNFORESEEN_ACCOUNT_CODE only; null there means no head is set (BUD-277).</summary>
        public long? AccountCodeId { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public string AuthorityRef { get; set; } = string.Empty;
    }
}
