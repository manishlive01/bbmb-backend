using BBMB.BudgetService.Authorization.Requirements;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ServiceDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Shared.Services.BudgetOffices
{
    /// <summary>Design §4.4 "Office". Reads active T-13 rows only.</summary>
    public sealed class BudgetOfficeScope(BudgetContext context, IOptionsMonitor<BudgetRightsOptions> options)
        : IBudgetOfficeScope
    {
        public async Task<ActorOffice?> ResolveActorOfficeAsync(
            UserChargeContext charge, string role, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(charge);
            if (string.IsNullOrWhiteSpace(role) ||
                !options.CurrentValue.RoleOfficeLevels.TryGetValue(role, out var level))
                return null;

            var offices = context.BudgetOffices.AsNoTracking().Where(o => o.IsActive && o.OfficeLevel == level);

            // employee_charge ce_id / cr_id / division_id hold master ids, not codes (design §4.4).
            offices = level switch
            {
                OfficeLevel.Division => charge.DivisionId is { } d ? offices.Where(o => o.DivisionId == d) : offices.Where(_ => false),
                OfficeLevel.Circle => charge.CRId is { } c ? offices.Where(o => o.CircleId == c) : offices.Where(_ => false),
                OfficeLevel.Ce => charge.CEId is { } e ? offices.Where(o => o.CeId == e) : offices.Where(_ => false),
                OfficeLevel.Finance or OfficeLevel.Secretariat or OfficeLevel.ItCell => offices,
                _ => offices.Where(_ => false),
            };

            // Two active rows for one by-role level would make the office ambiguous: refuse rather than pick.
            var found = await offices.Select(o => new ActorOffice(o.OfficeId, o.OfficeLevel)).Take(2).ToListAsync(ct);
            return found.Count == 1 ? found[0] : null;
        }

        public async Task<IReadOnlySet<long>> GetVisibleOfficeIdsAsync(long officeId, CancellationToken ct = default)
        {
            var level = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == officeId)
                .Select(o => o.OfficeLevel)
                .FirstOrDefaultAsync(ct);
            if (level is null)
                return new HashSet<long>();

            if (level is OfficeLevel.Finance or OfficeLevel.Secretariat)
            {
                var all = await context.BudgetOffices.AsNoTracking().Select(o => o.OfficeId).ToListAsync(ct);
                return all.ToHashSet();
            }

            var ids = await context.Database.SqlQuery<long>($"""
                WITH RECURSIVE tree AS (
                    SELECT office_id FROM budget.office WHERE office_id = {officeId}
                    UNION
                    SELECT o.office_id FROM budget.office o JOIN tree t ON o.parent_office_id = t.office_id
                )
                SELECT office_id AS "Value" FROM tree
                """).ToListAsync(ct);
            return ids.ToHashSet();
        }
    }
}
