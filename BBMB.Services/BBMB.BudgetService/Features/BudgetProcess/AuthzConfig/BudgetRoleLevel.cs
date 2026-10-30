using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>
    /// Decides a role's Budget office level type and whether the role is configured. The type is the role's stored
    /// <c>budget_role.office_level</c> code, one of <see cref="OfficeLevel.All"/> (DIVISION, CIRCLE, CE, SECRETARIAT,
    /// FINANCE, IT_CELL; the column carries the CHECK <c>ck_budget_role_office_level</c>). <c>office_level_id</c>
    /// references the <c>levels</c> master for display only; the level's name and <c>levels.status</c> are never
    /// interpreted. A role whose code is NULL or not one of the six is unconfigured and denied.
    /// </summary>
    public static class BudgetRoleLevel
    {
        /// <summary>The stored code when it is one of the six Budget office level codes (exact match), otherwise null.</summary>
        public static string? ToOfficeLevelCode(string? storedCode) =>
            storedCode is not null && OfficeLevel.All.Contains(storedCode, StringComparer.Ordinal) ? storedCode : null;

        /// <summary>
        /// D2: a role is authorised only when it is active in Budget, active in Auth SSO, has a valid office level
        /// code, and holds at least one active right. Anything else is unconfigured and gets no rights, no office
        /// and no acting-role candidacy.
        /// </summary>
        public static bool IsAuthorisable(bool isActive, bool ssoIsActive, string? officeLevelCode, bool hasActiveRight) =>
            isActive && ssoIsActive && officeLevelCode is not null && hasActiveRight;
    }
}
