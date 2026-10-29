using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>
    /// The ONE place that turns a role's <c>levels</c> master row into the Budget office level code
    /// (<see cref="OfficeLevel"/>: DIVISION, CIRCLE, CE, SECRETARIAT, FINANCE, IT_CELL) that
    /// <c>BudgetOfficeScope</c>, <c>ChargeResolver</c> and the actor read, and that decides whether a role is
    /// configured. The config snapshot, the admin list/configure endpoints and the migration's backfill all use
    /// the same rule: the level's name, trimmed, compared case-insensitively with the code. No other mapping
    /// (no aliasing such as "Chief Engineer" → CE) is applied; a level that does not match is "not a Budget
    /// office level" and a role on it stays unconfigured.
    /// <para>
    /// <b>Known limitation (review finding 8):</b> the level name is the authorisation key. Renaming a
    /// <c>levels</c> row (this service has no edit endpoint for it, but another system may) de-authorises every
    /// role on that level, and <c>budget.fn_config_version()</c> does not see the rename because <c>levels</c> has
    /// no audit trigger and is not part of it, so the cached snapshot keeps the old result until another config
    /// write or a restart. Removing this needs a code column on <c>levels</c> or an explicit mapping table.
    /// </para>
    /// </summary>
    public static class BudgetRoleLevel
    {
        /// <summary>The Budget office level code for a levels row, or null when it is inactive or does not match.</summary>
        public static string? ToOfficeLevelCode(string? levelName, bool levelIsActive)
        {
            if (!levelIsActive || string.IsNullOrWhiteSpace(levelName))
                return null;
            var trimmed = levelName.Trim();
            return OfficeLevel.All.FirstOrDefault(code => string.Equals(code, trimmed, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// D2: a role is authorised only when it is active in Budget, active in Auth SSO, has an office level that
        /// resolves to a Budget office level, and holds at least one active right. Anything else is
        /// unconfigured and gets no rights, no office and no acting-role candidacy.
        /// </summary>
        public static bool IsAuthorisable(bool isActive, bool ssoIsActive, string? officeLevelCode, bool hasActiveRight) =>
            isActive && ssoIsActive && officeLevelCode is not null && hasActiveRight;
    }
}
