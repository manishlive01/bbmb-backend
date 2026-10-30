#nullable disable
namespace BBMB.BudgetService.Migrations
{
    public partial class BudgetRoleSyncAndLevelFk
    {
        /// <summary>
        /// Frozen, migration-only copy of the one-time level-name alias list this APPLIED migration used for its
        /// backfill and Down. The migration file is not edited (it is applied on shared databases), and the name
        /// <c>BudgetRoleLevel</c> inside it resolves to this nested class first, so the SQL it emits is unchanged.
        /// Runtime code does not use level names: a role's type is <c>budget_role.office_level</c>.
        /// </summary>
        private static class BudgetRoleLevel
        {
            public static string SqlAliasValues() =>
                "VALUES ('CE', 'CE'), ('CIRCLE', 'CIRCLE'), ('CR', 'CIRCLE'), ('DIV', 'DIVISION'), ('DIVISION', 'DIVISION'), " +
                "('FINANCE', 'FINANCE'), ('IT_CELL', 'IT_CELL'), ('SECRETARIAT', 'SECRETARIAT')";
        }
    }
}
