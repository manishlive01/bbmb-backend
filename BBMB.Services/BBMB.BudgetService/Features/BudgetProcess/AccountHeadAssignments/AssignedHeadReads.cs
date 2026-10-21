using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments
{
    /// <summary>Live assignments of an office joined to T-07, ordered by complete code.</summary>
    public static class AssignedHeadReads
    {
        public static Task<List<AssignedHeadDto>> LiveAsync(BudgetContext context, long officeId, bool usableOnly, CancellationToken ct) =>
            context.AccountHeadAssignments.AsNoTracking()
                .Where(a => a.OfficeId == officeId && a.RemovedAt == null)
                .Join(context.HeadOfAccounts, a => a.AccountCodeId, h => h.AccountCodeId, (a, h) => new { a, h })
                .Where(x => !usableOnly || !x.h.NotForUse)
                .OrderBy(x => x.h.CompleteCode)
                .Select(x => new AssignedHeadDto(x.a.AssignmentId, x.a.OfficeId, x.h.AccountCodeId, x.h.CompleteCode, x.h.HeadDesc,
                    x.a.Source, x.h.NotForUse))
                .ToListAsync(ct);
    }
}
