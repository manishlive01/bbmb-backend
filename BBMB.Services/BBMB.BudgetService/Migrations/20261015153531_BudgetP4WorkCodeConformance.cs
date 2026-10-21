using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetP4WorkCodeConformance : Migration
    {
        private const string MigrationName = "BudgetP4WorkCodeConformance";
        private const string Gate = "work_code_reset";

        /// <inheritdoc />
        // Hand-ordered to design §4.6 steps 0-6 and §11.1 rules 2, 3, 8, 9. The order is load-bearing: the
        // estimate.work_id backfill and guard (step 1) run before any uniqueness drop (step 2); work_code_prev
        // is copied (step 3) before work_code is reset (step 4). estimate.work_code is KEPT (no DropColumn).
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---- Step 0: gate guard + pre-migration work count ----
            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_00_gate.sql"));

            // ---- Step 1: estimate.work_id added NULL (no default; the generated nullable:false/default:0 was
            // edited per §11.1 rule 2), backfilled from the old work_code match, then guarded ----
            migrationBuilder.AddColumn<int>(
                name: "work_id",
                schema: "budget",
                table: "estimate",
                type: "integer",
                nullable: true); // edited from nullable:false/defaultValue:0 (§4.6 step 1, §11.1 rule 3)

            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_01_estimate_backfill_guard.sql"));

            // ---- Step 2: SET NOT NULL (after the guard), then drop the FK, both uniqueness objects on
            // work_code and the estimate PK, then re-key estimate to (work_id, estimate_code) with FK to work.id ----
            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_02_estimate_work_id_not_null.sql"));

            migrationBuilder.DropForeignKey(
                name: "FK_estimate_work_work_code",
                schema: "budget",
                table: "estimate");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_work_work_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_work_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estimate",
                schema: "budget",
                table: "estimate");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estimate",
                schema: "budget",
                table: "estimate",
                columns: new[] { "work_id", "estimate_code" });

            migrationBuilder.AddForeignKey(
                name: "FK_estimate_work_work_id",
                schema: "budget",
                table: "estimate",
                column: "work_id",
                principalSchema: "budget",
                principalTable: "work",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // ---- Step 3: work_code_prev added, then copied from work_code UNDER THE MIGRATION ACTOR (audited:
            // one T-51 row per work). No existing value is lost ----
            migrationBuilder.AddColumn<int>(
                name: "work_code_prev",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName, Gate));
            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_03_work_code_prev_copy.sql"));

            // ---- Step 4: drop the view that depends on work.work_code (recreated after the type change),
            // then work_code -> varchar(12) NULL (int->text keeps every value), the four new code columns,
            // then UNDER THE MIGRATION ACTOR the reset to NULL (audited), the T-23 CHECKs, the partial unique
            // index, the DB-05 write-once trigger (all after the reset) and the view recreated ----
            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_04a_drop_dependent_views.sql"));

            migrationBuilder.AlterColumn<string>(
                name: "work_code",
                schema: "budget",
                table: "work",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 8);

            migrationBuilder.AddColumn<string>(
                name: "work_code_origin",
                schema: "budget",
                table: "work",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "work_code_occasion",
                schema: "budget",
                table: "work",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "work_code_fin_year",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "work_code_at",
                schema: "budget",
                table: "work",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName, Gate));
            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_04_work_code_reset.sql"));

            // ---- Step 5: T-24 work_code_serial + its CHECKs/FK + fn_generate_work_code ----
            migrationBuilder.CreateTable(
                name: "work_code_serial",
                schema: "budget",
                columns: table => new
                {
                    division_code = table.Column<int>(type: "integer", nullable: false),
                    fin_year = table.Column<int>(type: "integer", nullable: false),
                    last_serial = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_code_serial", x => new { x.division_code, x.fin_year });
                });

            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_05_serial_and_function.sql"));

            // ---- Step 6: triggers on T-24 (§11.2) and the in-migration exit guard ----
            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_06_serial_triggers.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP4WorkCodeConformance_07_exit_guard.sql"));
        }

        /// <inheritdoc />
        // Development databases only (design §11.1 rule 6). A production rollback is a database restore under
        // the backup arrangements (BUD-258). This Down drops the objects this migration created, restores
        // work_code from the preserved work_code_prev so a dev round-trip loses nothing, then reverses the EF
        // schema changes.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop this migration's SQL-created objects first (triggers before the function they call). The
            // view that depends on work.work_code is dropped so the type can be narrowed back, then recreated.
            migrationBuilder.Sql(
                "DROP VIEW IF EXISTS budget.v_fa_work_sanction; " +
                "DROP TRIGGER IF EXISTS trg_work_code_serial_require_actor ON budget.work_code_serial; " +
                "DROP TRIGGER IF EXISTS trg_work_code_serial_audit ON budget.work_code_serial; " +
                "DROP TRIGGER IF EXISTS trg_work_code_serial_forbid_stmt ON budget.work_code_serial; " +
                "DROP FUNCTION IF EXISTS budget.fn_generate_work_code(int, varchar, int); " +
                "DROP TRIGGER IF EXISTS trg_work_code_write_once ON budget.work; " +
                "DROP FUNCTION IF EXISTS budget.fn_work_code_write_once(); " +
                "DROP INDEX IF EXISTS budget.ux_work_work_code; " +
                "ALTER TABLE budget.work " +
                "DROP CONSTRAINT IF EXISTS fk_work_code_fin_year, " +
                "DROP CONSTRAINT IF EXISTS ck_work_code_origin_pair, " +
                "DROP CONSTRAINT IF EXISTS ck_work_code_origin_value, " +
                "DROP CONSTRAINT IF EXISTS ck_work_code_format, " +
                "DROP CONSTRAINT IF EXISTS ck_work_code_occasion, " +
                "DROP CONSTRAINT IF EXISTS ck_work_code_occasion_pair, " +
                "DROP CONSTRAINT IF EXISTS ck_work_code_at_pair, " +
                "DROP CONSTRAINT IF EXISTS ck_work_code_fin_year;");

            // Restore the live code from the preserved column so a dev round-trip keeps every value. Done with
            // triggers off for this single dev-only restore (session-local); the DB-05 trigger is already dropped.
            migrationBuilder.Sql(
                "SET session_replication_role = replica; " +
                "UPDATE budget.work SET work_code = work_code_prev::text WHERE work_code_prev IS NOT NULL; " +
                "SET session_replication_role = origin;");

            migrationBuilder.DropForeignKey(
                name: "FK_estimate_work_work_id",
                schema: "budget",
                table: "estimate");

            migrationBuilder.DropTable(
                name: "work_code_serial",
                schema: "budget");

            migrationBuilder.DropPrimaryKey(
                name: "PK_estimate",
                schema: "budget",
                table: "estimate");

            migrationBuilder.DropColumn(
                name: "work_code_at",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "work_code_fin_year",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "work_code_occasion",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "work_code_origin",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "work_code_prev",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "work_id",
                schema: "budget",
                table: "estimate");

            migrationBuilder.AlterColumn<int>(
                name: "work_code",
                schema: "budget",
                table: "work",
                type: "integer",
                maxLength: 8,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "character varying(12)",
                oldMaxLength: 12,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_work_work_code",
                schema: "budget",
                table: "work",
                column: "work_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_estimate",
                schema: "budget",
                table: "estimate",
                columns: new[] { "work_code", "estimate_code" });

            migrationBuilder.CreateIndex(
                name: "IX_work_work_code",
                schema: "budget",
                table: "work",
                column: "work_code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_estimate_work_work_code",
                schema: "budget",
                table: "estimate",
                column: "work_code",
                principalSchema: "budget",
                principalTable: "work",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Recreate V-08 reading the restored int work_code (dev-only round-trip; design §11.1 rule 6).
            migrationBuilder.Sql(
                "CREATE VIEW budget.v_fa_work_sanction AS " +
                "SELECT w.id AS work_id, w.work_code, w.division_id, w.work_class, w.work_desc, " +
                "a.admin_approval_id, av.version_no AS aa_version_no, av.aa_reference_no, av.date_of_approval, " +
                "av.amount_sanctioned AS aa_amount_sanctioned, av.account_code_id AS aa_account_code_id, " +
                "t.technical_sanction_id, t.tab, tv.version_no AS ts_version_no, tv.ts_number, tv.ts_date, " +
                "tv.ts_amount, t.estimate_code, t.estimate_code_at, " +
                "(t.technical_sanction_id IS NOT NULL AND t.estimate_code IS NULL) AS awaiting_estimate_code " +
                "FROM budget.work w " +
                "LEFT JOIN budget.admin_approval a ON a.work_id = w.id " +
                "LEFT JOIN budget.admin_approval_version av ON av.admin_approval_id = a.admin_approval_id AND av.version_no = a.current_version_no " +
                "LEFT JOIN budget.technical_sanction t ON t.work_id = w.id " +
                "LEFT JOIN budget.technical_sanction_version tv ON tv.technical_sanction_id = t.technical_sanction_id AND tv.version_no = t.current_version_no " +
                "WHERE a.admin_approval_id IS NOT NULL OR t.technical_sanction_id IS NOT NULL;");
        }
    }
}
