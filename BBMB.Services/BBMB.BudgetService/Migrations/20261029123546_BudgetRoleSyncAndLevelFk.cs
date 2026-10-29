using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <summary>
    /// Dynamic Budget role management (D1, D3):
    /// <list type="bullet">
    /// <item><c>budget_role.office_level_id</c> — nullable FK to the existing <c>budget.levels</c> master (RESTRICT);
    /// backfilled from the legacy <c>office_level</c> string by name; no row is ever inserted into the master;</item>
    /// <item><c>sso_role_id</c> (unique, nullable for legacy rows), <c>sso_is_active</c> (default true) and
    /// <c>last_synced_at</c> for the idempotent Auth SSO role sync;</item>
    /// <item><c>budget_project_settings.sso_project_id</c> set to the Budget project's SSO id where still NULL;</item>
    /// <item>the legacy <c>office_level</c> string column (and its CHECK) dropped — every consumer reads the FK.</item>
    /// </list>
    /// The existing row-level actor/audit triggers on <c>budget_role</c> audit the whole row, so the new columns
    /// are audited with no trigger change, and <c>budget.fn_config_version()</c> already covers the table.
    /// Not applied to any database by this change.
    /// </summary>
    public partial class BudgetRoleSyncAndLevelFk : Migration
    {
        private const string MigrationName = "BudgetRoleSyncAndLevelFk";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            // Data shaping under the named migration actor, so each UPDATE is stamped and audited (§11.1 rule 8):
            // office_level -> office_level_id by levels name (unmatched => NULL + WARNING), and sso_project_id.
            // An active administrator role left without an active levels row aborts the migration unless
            // bbmb_gate.role_level_unmapped_admin is set (development and tests only): it would lock Budget
            // administration out.
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName));
            migrationBuilder.Sql(SqlResource.Read("BudgetRoleSyncAndLevelFk_01_backfill.sql"));

            // Every consumer now reads office_level_id (BudgetConfigProvider, ConfigSlices); the string goes, and
            // its CHECK ck_budget_role_office_level with it.
            migrationBuilder.DropColumn(
                name: "office_level",
                schema: "budget",
                table: "budget_role");
        }

        /// <inheritdoc />
        // Development databases only (per the existing convention). Restores office_level from the FK, then drops
        // the new columns. office_level comes back nullable: a role that had no level is not given one.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "office_level",
                schema: "budget",
                table: "budget_role",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName));
            migrationBuilder.Sql(SqlResource.Read("BudgetRoleSyncAndLevelFk_down.sql"));

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
        }
    }
}
