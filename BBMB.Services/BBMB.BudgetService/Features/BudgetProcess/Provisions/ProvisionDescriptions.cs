using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Provisions
{
    /// <summary>
    /// What a provision is, in words: the holder (work, head or unforeseen line), its name, account code and
    /// office (BUD-207, BUD-300; BE-GAP-01/02). Name: the work's description for WORK, the head's description for
    /// HEAD, the demanded unforeseen nomenclature for UNFORESEEN (BUD-277).
    /// </summary>
    public sealed record ProvisionDescription(
        long ProvisionId, int FinYear, string Basis, string Category, string HolderType,
        int? WorkId, string? WorkCode, long OfficeId, string? OfficeName,
        long? AccountCodeId, string? CompleteCode, string? Name, bool IsSuperseded);
    public static class ProvisionDescriptions
    {
        public static async Task<Dictionary<long, ProvisionDescription>> ResolveAsync(
            BudgetContext context, IEnumerable<long> provisionIds, CancellationToken ct = default)
        {
            var ids = provisionIds.Distinct().ToList();
            if (ids.Count == 0)
                return [];
            var rows = await (
                from p in context.Provisions.AsNoTracking()
                where ids.Contains(p.ProvisionId)
                join w in context.Works.AsNoTracking() on p.WorkId equals (int?)w.Id into wj
                from w in wj.DefaultIfEmpty()
                join h in context.HeadOfAccounts.AsNoTracking() on p.AccountCodeId equals (long?)h.AccountCodeId into hj
                from h in hj.DefaultIfEmpty()
                join d in context.DemandLines.AsNoTracking() on p.DemandLineId equals (long?)d.DemandLineId into dj
                from d in dj.DefaultIfEmpty()
                select new
                {
                    p.ProvisionId, p.FinYear, p.Basis, p.Category, p.HolderType, p.WorkId, p.OfficeId, p.AccountCodeId,
                    p.SupersededByProvisionId,
                    WorkCode = w == null ? null : w.WorkCode,
                    WorkDesc = w == null ? null : w.WorkDesc,
                    CompleteCode = h == null ? null : h.CompleteCode,
                    HeadDesc = h == null ? null : h.HeadDesc,
                    UnforeseenName = d == null ? null : d.UnforeseenName,
                }).ToListAsync(ct);
            var offices = await BudgetOfficeNames.ResolveAsync(context, rows.Select(r => r.OfficeId), ct);
            return rows.ToDictionary(r => r.ProvisionId, r => new ProvisionDescription(
                r.ProvisionId, r.FinYear, r.Basis, r.Category, r.HolderType, r.WorkId, r.WorkCode, r.OfficeId,
                offices.GetValueOrDefault(r.OfficeId), r.AccountCodeId, r.CompleteCode,
                r.HolderType switch
                {
                    ProvisionHolderType.Work => r.WorkDesc,
                    ProvisionHolderType.Head => r.HeadDesc,
                    ProvisionHolderType.Unforeseen => r.UnforeseenName ?? "Unforeseen work",
                    _ => null,
                },
                r.SupersededByProvisionId != null));
        }
    }
}
