using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Cycles
{
    /// <summary>
    /// Design §6.2 head-wise expected set from P2: every active DIVISION, CIRCLE or CE office holding a live T-21
    /// assignment. Which head-wise category an assignment belongs to is not stated (OI-43), so such an office is expected
    /// in every head-wise category opened; an office with no demand submits a nil proposal.
    /// </summary>
    public static class HeadWiseExpectedSet
    {
        public static async Task<IReadOnlyList<long>> AssignedOfficesAsync(BudgetContext context, CancellationToken ct = default) =>
            await context.AccountHeadAssignments.AsNoTracking()
                .Where(a => a.RemovedAt == null)
                .Join(context.BudgetOffices.Where(o => o.IsActive &&
                        (o.OfficeLevel == OfficeLevel.Division || o.OfficeLevel == OfficeLevel.Circle || o.OfficeLevel == OfficeLevel.Ce)),
                    a => a.OfficeId, o => o.OfficeId, (a, o) => o.OfficeId)
                .Distinct().ToListAsync(ct);
    }
}
