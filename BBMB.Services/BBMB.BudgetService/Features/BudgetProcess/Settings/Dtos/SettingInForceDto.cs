namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos
{
    /// <summary>The T-17 row in force for one key at an instant (design §5.3).</summary>
    public sealed record SettingInForceDto(
        long SettingId, string SettingKey, decimal? NumericValue, long? AccountCodeId, DateTimeOffset EffectiveFrom, string? AuthorityRef);
}
