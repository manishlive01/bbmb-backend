using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings
{
    /// <summary>
    /// Reads T-17 (design §5.3): the value in force for a key is the latest row with
    /// <c>effective_from &lt;= at</c>; a later row is not in force until its instant (BUD-242).
    /// </summary>
    public interface IBudgetSettingsReader
    {
        /// <summary>One row per key that has a row in force at <paramref name="at"/>.</summary>
        Task<IReadOnlyList<SettingInForceDto>> GetInForceAsync(DateTimeOffset at, CancellationToken ct = default);
        /// <summary>The row in force for <paramref name="key"/> at <paramref name="at"/>, or null.</summary>
        Task<SettingInForceDto?> GetAsync(string key, DateTimeOffset at, CancellationToken ct = default);
    }
}
