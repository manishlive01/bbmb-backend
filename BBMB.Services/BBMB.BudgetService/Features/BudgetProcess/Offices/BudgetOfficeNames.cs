using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices
{
    /// <summary>
    /// The display name of T-13 offices (DDD T-13: a mapped office's name is read from its master; Secretariat,
    /// Finance and IT Cell carry their own).
    /// </summary>
    public static class BudgetOfficeNames
    {
        public static async Task<Dictionary<long, string?>> ResolveAsync(
            BudgetContext context, IEnumerable<long> officeIds, CancellationToken ct = default)
        {
            var ids = officeIds.Distinct().ToList();
            return await (
                from o in context.BudgetOffices.AsNoTracking()
                where ids.Contains(o.OfficeId)
                join d in context.Divisions on o.DivisionId equals (int?)d.Id into dj
                from d in dj.DefaultIfEmpty()
                join cr in context.CRManagements on o.CircleId equals (int?)cr.Id into crj
                from cr in crj.DefaultIfEmpty()
                join ce in context.CEManagements on o.CeId equals (int?)ce.Id into cej
                from ce in cej.DefaultIfEmpty()
                select new { o.OfficeId, Name = o.OfficeName ?? d.DivisionDesc ?? cr.Name ?? ce.Name })
                .ToDictionaryAsync(x => x.OfficeId, x => (string?)x.Name, ct);
        }
    }
}
