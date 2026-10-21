using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings
{
    /// <summary>
    /// Reads T-18 (design §4.11, BUD-025): the mode in force for an office and year is the latest row with
    /// <c>effective_from &lt;= at</c>. The absence of any row means KEYED, the Financial Accounting module not
    /// yet being available (DDD T-18).
    /// </summary>
    public static class ExpenditureSourceReads
    {
        public static async Task<string> ModeInForceAsync(
            BudgetContext context, long officeId, int finYear, DateTimeOffset at, CancellationToken ct)
        {
            var mode = await context.ExpenditureSourceSettings.AsNoTracking()
                .Where(s => s.OfficeId == officeId && s.FinYear == finYear && s.EffectiveFrom <= at)
                .OrderByDescending(s => s.EffectiveFrom)
                .Select(s => s.Mode)
                .FirstOrDefaultAsync(ct);
            return mode ?? ExpenditureSourceMode.Keyed;
        }
    }
}
