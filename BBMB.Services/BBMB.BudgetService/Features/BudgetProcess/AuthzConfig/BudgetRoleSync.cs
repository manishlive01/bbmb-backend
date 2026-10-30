using BBMB.BudgetService.Authorization.RoleSource;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>Counts returned by a role sync (the contract of <c>POST /api/budget-config/roles/sync</c>).</summary>
    public sealed record RoleSyncResultDto(int Created, int Updated, int Deactivated, int Unchanged);

    /// <summary>The planned change set of one sync: counts, rows to insert, and name clashes that were skipped.</summary>
    public sealed record RoleSyncPlan(RoleSyncResultDto Counts, IReadOnlyList<BudgetRole> NewRoles, IReadOnlyList<string> SkippedNames)
    {
        public bool HasChanges => Counts.Created + Counts.Updated + Counts.Deactivated > 0;
    }

    /// <summary>
    /// Idempotent merge of the Auth SSO role catalogue into <c>budget_role</c> (D3). Pure: it edits the tracked
    /// rows it is given and returns the rows to insert; the caller saves inside an audited action scope.
    /// <list type="bullet">
    /// <item>match on <c>sso_role_id</c> first, then on <c>role_name</c> (case-insensitive) among rows that have no
    /// SSO id yet, which adopts a legacy seeded row;</item>
    /// <item>a matched row gets the SSO name and active flag (an adopted row also gets its SSO id);</item>
    /// <item>an unmatched SSO role is inserted with <c>office_level_id</c> NULL, <c>is_active</c> true, no rights:
    /// unconfigured, therefore denied (D2);</item>
    /// <item>a row whose SSO role is absent from the response gets <c>sso_is_active</c> false;</item>
    /// <item><c>office_level_id</c>, <c>is_admin</c>, <c>is_active</c>, rights, query fields and admin assignments
    /// are never touched — they are Budget-owned.</item>
    /// </list>
    /// <c>last_synced_at</c> is stamped only on a row the sync changes, so a repeated sync with no SSO change
    /// writes nothing (no UPDATE, no audit row, no cache rebuild).
    /// <para>
    /// A name that already belongs to a different row (another SSO id) cannot be inserted or renamed to — the
    /// <c>ux_budget_role_name</c> unique index — so that SSO role is skipped, listed in
    /// <see cref="RoleSyncPlan.SkippedNames"/> and logged by the caller; it is not counted.
    /// </para>
    /// <para>
    /// Auth SSO migration <c>ProjectWiseRoleName</c> made a role name unique per project
    /// (<c>auth."AspNetRoles"</c> index <c>RoleNameIndex</c> on (NormalizedName, ProjectMasterId) WHERE NOT IsDeleted),
    /// so the same name may now exist in several projects. The sync reads ONE project
    /// (<c>POST /api/roles/by-project</c> with the Budget project id), within which names stay unique, so the global
    /// <c>ux_budget_role_name</c> index still holds. Roles are matched on the SSO role id first for the same reason.
    /// </para>
    /// <para>
    /// A row inserted here has <c>office_level_id</c> and the legacy <c>office_level</c> string NULL. The previous
    /// release reads that string as required; do not run a sync against a database the previous release still
    /// serves (expand/contract window, open item in the migration report).
    /// </para>
    /// </summary>
    public static class BudgetRoleSync
    {
        public static RoleSyncPlan Plan(IReadOnlyList<BudgetRole> existing, IReadOnlyList<SsoRole> ssoRoles, DateTimeOffset now)
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(ssoRoles);
            int created = 0, updated = 0, deactivated = 0, unchanged = 0;
            var newRoles = new List<BudgetRole>();
            var skipped = new List<string>();
            var matched = new HashSet<BudgetRole>(ReferenceEqualityComparer.Instance);

            // One entry per SSO role id (a duplicate in the response is ignored after the first).
            var distinct = ssoRoles.GroupBy(r => r.RoleId).Select(g => g.First()).ToList();
            foreach (var sso in distinct)
            {
                var row = existing.FirstOrDefault(r => r.SsoRoleId == sso.RoleId)
                    ?? existing.FirstOrDefault(r => r.SsoRoleId is null && !matched.Contains(r)
                        && string.Equals(r.RoleName, sso.RoleName, StringComparison.OrdinalIgnoreCase));
                var nameOwner = existing.Concat(newRoles).FirstOrDefault(r => !ReferenceEquals(r, row)
                    && string.Equals(r.RoleName, sso.RoleName, StringComparison.OrdinalIgnoreCase));

                if (row is null)
                {
                    if (nameOwner is not null)
                    {
                        skipped.Add(sso.RoleName);
                        continue;
                    }
                    newRoles.Add(new BudgetRole
                    {
                        RoleName = sso.RoleName,
                        SsoRoleId = sso.RoleId,
                        SsoIsActive = sso.IsActive,
                        LastSyncedAt = now,
                        IsActive = true,
                        IsAdmin = false,
                        OfficeLevelId = null,
                    });
                    created++;
                    continue;
                }

                matched.Add(row);
                var changed = false;
                if (row.SsoRoleId != sso.RoleId)
                {
                    row.SsoRoleId = sso.RoleId; // adoption of a legacy seeded row
                    changed = true;
                }
                if (!string.Equals(row.RoleName, sso.RoleName, StringComparison.Ordinal))
                {
                    if (nameOwner is null)
                    {
                        row.RoleName = sso.RoleName;
                        changed = true;
                    }
                    else
                    {
                        skipped.Add(sso.RoleName);
                    }
                }
                if (row.SsoIsActive != sso.IsActive)
                {
                    row.SsoIsActive = sso.IsActive;
                    changed = true;
                }
                if (changed)
                {
                    row.LastSyncedAt = now;
                    updated++;
                }
                else
                {
                    unchanged++;
                }
            }

            foreach (var row in existing.Where(r => !matched.Contains(r)))
            {
                if (row.SsoIsActive)
                {
                    row.SsoIsActive = false;
                    row.LastSyncedAt = now;
                    deactivated++;
                }
                else
                {
                    unchanged++;
                }
            }

            return new RoleSyncPlan(new RoleSyncResultDto(created, updated, deactivated, unchanged), newRoles, skipped);
        }
    }
}
