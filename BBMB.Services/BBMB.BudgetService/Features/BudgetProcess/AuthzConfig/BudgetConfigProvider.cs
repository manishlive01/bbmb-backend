using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>
    /// DB-backed <see cref="IBudgetConfigProvider"/> (design §2). On read, if the cache is fresh it is served;
    /// otherwise a cheap version stamp (<c>budget.fn_config_version()</c>) is read first — unchanged ⇒ the TTL
    /// window is extended without a rebuild; changed (or nothing cached) ⇒ the snapshot is rebuilt from the
    /// seven tables and stored. Fails closed: no active settings row ⇒ <see cref="BudgetConfigSnapshot.Empty"/>
    /// (logged once per load); a DB error bubbles to the global handler, no stale snapshot is served past TTL.
    /// </summary>
    public sealed class BudgetConfigProvider(
        BudgetContext context,
        BudgetConfigCache cache,
        ILogger<BudgetConfigProvider> logger) : IBudgetConfigProvider
    {
        public async Task<BudgetConfigSnapshot> GetAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            if (cache.TryGetFresh(now, out var fresh, out _))
                return fresh;

            var version = await ReadVersionStampAsync(ct);
            if (cache.CachedVersion == version && cache.CachedSnapshot is { } cached)
            {
                cache.Touch(now);
                return cached;
            }

            var snapshot = await BuildSnapshotAsync(ct);
            cache.Store(snapshot, version, now);
            return snapshot;
        }

        public void Invalidate() => cache.Invalidate();

        private async Task<long> ReadVersionStampAsync(CancellationToken ct)
        {
            // One round-trip, monotonic: the max audit_log id touching the seven tables (design §2, OQ-1).
            var rows = await context.Database
                .SqlQueryRaw<long>("SELECT budget.fn_config_version() AS \"Value\"")
                .ToListAsync(ct);
            return rows.Count > 0 ? rows[0] : 0;
        }

        private async Task<BudgetConfigSnapshot> BuildSnapshotAsync(CancellationToken ct)
        {
            var settings = await context.BudgetProjectSettings.AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.Id)
                .FirstOrDefaultAsync(ct);
            if (settings is null)
            {
                logger.LogWarning("No active budget_project_settings row: budget authorization fails closed (M-20).");
                return BudgetConfigSnapshot.Empty;
            }

            var roles = await context.BudgetRoles.AsNoTracking().Where(r => r.IsActive).ToListAsync(ct);
            var rights = await context.BudgetRights.AsNoTracking().Where(r => r.IsActive).ToListAsync(ct);
            var queryFields = await context.BudgetQueryFields.AsNoTracking().Where(q => q.IsActive).ToListAsync(ct);
            var roleRights = await context.BudgetRoleRights.AsNoTracking().ToListAsync(ct);
            var roleQueryFields = await context.BudgetRoleQueryFields.AsNoTracking().ToListAsync(ct);
            var assignments = await context.BudgetAdminRoleAssignments.AsNoTracking()
                .Where(a => a.Status != AdminAssignmentStatus.Revoked)
                .ToListAsync(ct);

            var roleById = roles.ToDictionary(r => r.Id);
            var rightById = rights.ToDictionary(r => r.Id);
            var fieldById = queryFields.ToDictionary(q => q.Id);

            var roleOfficeLevels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in roles)
                roleOfficeLevels[r.RoleName] = r.OfficeLevel;

            // Right key -> role names holding it.
            var rightsMap = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var right in rights)
                rightsMap[right.RightKey] = [];
            foreach (var rr in roleRights)
            {
                if (rightById.TryGetValue(rr.RightId, out var right) && roleById.TryGetValue(rr.RoleId, out var role))
                    rightsMap[right.RightKey].Add(role.RoleName);
            }

            // Query field -> role names.
            var queryFieldMap = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var field in queryFields)
                queryFieldMap[field.FieldName] = [];
            foreach (var rq in roleQueryFields)
            {
                if (fieldById.TryGetValue(rq.QueryFieldId, out var field) && roleById.TryGetValue(rq.RoleId, out var role))
                    queryFieldMap[field.FieldName].Add(role.RoleName);
            }

            // Effective admins: role.is_admin AND an APPROVED, document-backed assignment (design §1).
            var effectiveAdminRoleIds = assignments
                .Where(a => a.IsEffectiveAdmin())
                .Select(a => a.RoleId)
                .ToHashSet();
            var adminRoles = roles
                .Where(r => r.IsAdmin && effectiveAdminRoleIds.Contains(r.Id))
                .Select(r => r.RoleName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new BudgetConfigSnapshot
            {
                SsoProjectId = settings.SsoProjectId,
                SsoProjectName = settings.SsoProjectName,
                ProjectCode = settings.EmployeeChargeProjectCode,
                EditLeaseMinutes = settings.EditLeaseMinutes,
                RoleOfficeLevels = roleOfficeLevels,
                Rights = rightsMap.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray(), StringComparer.Ordinal),
                QueryFieldRoles = queryFieldMap.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray(), StringComparer.Ordinal),
                AdminRoles = adminRoles,
            };
        }
    }
}
