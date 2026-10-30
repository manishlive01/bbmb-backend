using System;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace BBMB.BudgetService.Migrations
{
    /// <summary>
    /// Dynamic Budget role management, EXPAND step of an expand/contract change (D2):
    /// <list type="bullet">
    /// <item><c>budget_role.office_level_id</c> — nullable FK to the existing <c>budget.levels</c> master (RESTRICT);
    /// backfilled from the legacy <c>office_level</c> string through <see cref="BudgetRoleLevel.Aliases"/>;</item>
    /// <item><c>budget.levels</c> gains FINANCE, SECRETARIAT and IT_CELL, each only where no row of that name exists,
    /// with <c>status = false</c> like the existing rows; existing rows are not updated (D4);</item>
    /// <item><c>sso_role_id</c> (unique, nullable for legacy rows), <c>sso_is_active</c> (default true) and
    /// <c>last_synced_at</c> for the idempotent Auth SSO role sync;</item>
    /// <item><c>budget_project_settings.sso_project_id</c> set to the Budget project's SSO id where still NULL;</item>
    /// <item>the legacy <c>office_level</c> string is KEPT and made nullable. The previous release still reads it
    /// from the same database, and the application dual-writes it. Its CHECK <c>ck_budget_role_office_level</c> is
    /// kept: a CHECK passes on NULL, so no relaxation is needed. Dropping the column is the CONTRACT step, a later
    /// migration once the previous release is no longer deployed.</item>
    /// </list>
    /// Ordered after <c>20261029044243_ProjectWiseRoleName</c>, which is the Auth SSO service's migration recorded in
    /// the shared <c>public."__EFMigrationsHistory"</c> (it changed <c>auth."AspNetRoles"</c>, not the budget schema);
    /// EF ignores history rows of other contexts, so the order is by timestamp only.
    /// The existing row-level actor/audit triggers on <c>budget_role</c> audit the whole row, so the new columns are
    /// audited with no trigger change, and <c>budget.fn_config_version()</c> already covers the table.
    /// </summary>
    public partial class BudgetRoleSyncAndLevelFk : Migration
    {
        private const string MigrationName = "BudgetRoleSyncAndLevelFk";
        private const string AliasToken = "{{LEVEL_ALIASES}}";

        /// <summary>An embedded SQL file with the alias list of <see cref="BudgetRoleLevel"/> substituted.</summary>
        internal static string WithAliases(string fileName) =>
            SqlResource.Read(fileName).Replace(AliasToken, BudgetRoleLevel.SqlAliasValues(), StringComparison.Ordinal);

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "office_level",
                schema: "budget",
                table: "budget_role",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12);
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_synced_at",
                schema: "budget",
                table: "budget_role",
                type: "timestamp with time zone",
                nullable: true);
            migrationBuilder.AddColumn<int>(
                name: "office_level_id",
                schema: "budget",
                table: "budget_role",
                type: "integer",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
                name: "sso_is_active",
                schema: "budget",
                table: "budget_role",
                type: "boolean",
                nullable: false,
                defaultValue: true);
            migrationBuilder.AddColumn<Guid>(
                name: "sso_role_id",
                schema: "budget",
                table: "budget_role",
                type: "uuid",
                nullable: true);
            migrationBuilder.CreateIndex(
                name: "ix_budget_role_office_level_id",
                schema: "budget",
                table: "budget_role",
                column: "office_level_id");
            migrationBuilder.CreateIndex(
                name: "ux_budget_role_sso_role_id",
                schema: "budget",
                table: "budget_role",
                column: "sso_role_id",
                unique: true);
            migrationBuilder.AddForeignKey(
                name: "fk_budget_role_office_level",
                schema: "budget",
                table: "budget_role",
                column: "office_level_id",
                principalSchema: "budget",
                principalTable: "levels",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            // D4: the Budget-only levels, insert-if-missing (budget.levels has no actor/audit trigger).
            migrationBuilder.Sql(SqlResource.Read("BudgetRoleSyncAndLevelFk_01_levels.sql"));
            // D5: data shaping under the named migration actor, so each UPDATE is stamped and audited (§11.1 rule 8):
            // office_level -> office_level_id through the alias map (unmatched => NULL + WARNING), and
            // sso_project_id. An active administrator role left without a levels row aborts the migration unless
            // bbmb_gate.role_level_unmapped_admin is set (development and tests only): it would lock Budget
            // administration out.
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName));
            migrationBuilder.Sql(WithAliases("BudgetRoleSyncAndLevelFk_02_backfill.sql"));
        }

        /// <inheritdoc />
        // Development databases only (per the existing convention). Fills any NULL office_level from the FK, then
        // drops the new columns and restores NOT NULL. A role with no level at all aborts Down (see the SQL).
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName));
            migrationBuilder.Sql(WithAliases("BudgetRoleSyncAndLevelFk_down.sql"));
            migrationBuilder.DropForeignKey(
                name: "fk_budget_role_office_level",
                schema: "budget",
                table: "budget_role");
            migrationBuilder.DropIndex(
                name: "ix_budget_role_office_level_id",
                schema: "budget",
                table: "budget_role");
            migrationBuilder.DropIndex(
                name: "ux_budget_role_sso_role_id",
                schema: "budget",
                table: "budget_role");
            migrationBuilder.DropColumn(
                name: "last_synced_at",
                schema: "budget",
                table: "budget_role");
            migrationBuilder.DropColumn(
                name: "office_level_id",
                schema: "budget",
                table: "budget_role");
            migrationBuilder.DropColumn(
                name: "sso_is_active",
                schema: "budget",
                table: "budget_role");
            migrationBuilder.DropColumn(
                name: "sso_role_id",
                schema: "budget",
                table: "budget_role");
            // No defaultValue: a '' default would violate ck_budget_role_office_level; the SQL above guarantees no NULL.
            migrationBuilder.AlterColumn<string>(
                name: "office_level",
                schema: "budget",
                table: "budget_role",
                type: "character varying(12)",
                maxLength: 12,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12,
                oldNullable: true);
        }
    }
}
