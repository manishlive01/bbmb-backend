using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings
{
    public sealed class BudgetSettingsReader(BudgetContext context) : IBudgetSettingsReader
    {
        public async Task<IReadOnlyList<SettingInForceDto>> GetInForceAsync(DateTimeOffset at, CancellationToken ct = default)
        {
            var rows = await context.BudgetSettings.AsNoTracking()
                .Where(s => s.EffectiveFrom <= at)
                .Select(s => new SettingInForceDto(s.SettingId, s.SettingKey, s.NumericValue, s.AccountCodeId, s.EffectiveFrom, s.AuthorityRef))
                .ToListAsync(ct);
            // Latest effective_from per key; the UK (key, effective_from) makes it unique.
            return rows.GroupBy(r => r.SettingKey)
                .Select(g => g.OrderByDescending(r => r.EffectiveFrom).First())
                .OrderBy(r => r.SettingKey, StringComparer.Ordinal)
                .ToList();
        }

        public Task<SettingInForceDto?> GetAsync(string key, DateTimeOffset at, CancellationToken ct = default) =>
            context.BudgetSettings.AsNoTracking()
                .Where(s => s.SettingKey == key && s.EffectiveFrom <= at)
                .OrderByDescending(s => s.EffectiveFrom)
                .Select(s => new SettingInForceDto(s.SettingId, s.SettingKey, s.NumericValue, s.AccountCodeId, s.EffectiveFrom, s.AuthorityRef))
                .FirstOrDefaultAsync(ct);
    }
}
