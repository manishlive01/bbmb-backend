using BBMB.BudgetService.Features.BudgetProcess.Settings.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings
{
    /// <summary>
    /// Reads T-19 (design §5.3): the limit in force for a level on a date is the row with the latest
    /// <c>effective_from &lt;= date</c>. No row, or a row with a null amount, means no limit. Used by
    /// AdminApprovalRules.CheckDelegationAsync (BUD-055, M-27).
    /// </summary>
    public interface IDelegationLimitReader
    {
        Task<DelegationLimitDto?> GetInForceAsync(string authorityLevel, DateOnly on, CancellationToken ct = default);
        /// <summary>Every row, by level then date, each marked whether it is the one in force on <paramref name="on"/>.</summary>
        Task<IReadOnlyList<DelegationLimitDto>> GetAllAsync(DateOnly on, CancellationToken ct = default);
    }

    public sealed class DelegationLimitReader(BudgetContext context) : IDelegationLimitReader
    {
        public Task<DelegationLimitDto?> GetInForceAsync(string authorityLevel, DateOnly on, CancellationToken ct = default) =>
            context.DelegationLimits.AsNoTracking()
                .Where(d => d.AuthorityLevel == authorityLevel && d.EffectiveFrom <= on)
                .OrderByDescending(d => d.EffectiveFrom)
                .Select(d => new DelegationLimitDto(d.DelegationLimitId, d.AuthorityLevel, d.MaxAmount, d.EffectiveFrom, d.SourceRef, true))
                .FirstOrDefaultAsync(ct);

        public async Task<IReadOnlyList<DelegationLimitDto>> GetAllAsync(DateOnly on, CancellationToken ct = default)
        {
            var rows = await context.DelegationLimits.AsNoTracking()
                .OrderBy(d => d.AuthorityLevel).ThenBy(d => d.EffectiveFrom)
                .Select(d => new DelegationLimitDto(d.DelegationLimitId, d.AuthorityLevel, d.MaxAmount, d.EffectiveFrom, d.SourceRef, false))
                .ToListAsync(ct);
            var inForce = rows.Where(r => r.EffectiveFrom <= on)
                .GroupBy(r => r.AuthorityLevel)
                .Select(g => g.MaxBy(r => r.EffectiveFrom)!.DelegationLimitId)
                .ToHashSet();
            return rows.Select(r => r with { InForce = inForce.Contains(r.DelegationLimitId) }).ToList();
        }
    }
}
