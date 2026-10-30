using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
namespace BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts
{
    /// <summary>
    /// The read-only drift check between <c>budget_role.office_level_id</c> and the legacy <c>office_level</c>
    /// string (review R2-2). Run it before this release replaces the previous one and again immediately before the
    /// contract migration that drops <c>office_level</c>; every returned row is resolved through roles/configure
    /// first. See <c>Sql/BudgetRoleLevel_drift_check.sql</c> for the drift kinds.
    /// </summary>
    public static class BudgetRoleLevelDriftCheck
    {
        public const string FileName = "BudgetRoleLevel_drift_check.sql";
        private const string AliasToken = "{{LEVEL_ALIASES}}";

        /// <summary>The check's SQL with the alias list of <see cref="BudgetRoleLevel.Aliases"/> substituted.</summary>
        public static string Sql() =>
            SqlResource.Read(FileName).Replace(AliasToken, BudgetRoleLevel.SqlAliasValues(), StringComparison.Ordinal);
    }
}
