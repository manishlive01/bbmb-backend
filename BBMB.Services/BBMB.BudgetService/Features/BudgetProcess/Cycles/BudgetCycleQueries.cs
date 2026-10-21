using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles
{
    /// <summary>
    /// Design §5.3: the current cycle is the T-14 row with status OPEN and the greatest current_fin_year; no such
    /// row means there is no current cycle. Every use of "current cycle" goes through this query.
    /// </summary>
    public static class BudgetCycleQueries
    {
        public static Task<BudgetCycle?> GetCurrentAsync(BudgetContext context, CancellationToken ct = default) =>
            context.BudgetCycles.AsNoTracking()
                .Where(c => c.Status == CycleStatus.Open)
                .OrderByDescending(c => c.CurrentFinYear)
                .FirstOrDefaultAsync(ct);
    }
}
