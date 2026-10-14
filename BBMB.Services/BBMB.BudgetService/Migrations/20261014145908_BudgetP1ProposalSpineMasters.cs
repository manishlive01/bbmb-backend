using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetP1ProposalSpineMasters : Migration
    {
        private const string MigrationName = "BudgetP1ProposalSpineMasters";
        private const string Gate = "p1_ddd_deviations";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-ordered to design §5.2 (T-07 order, work order) and §11.1 rules 2, 8, 9 (PD-1).

            // Step 0: gate guard, the first statement.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_00_gate.sql"));

            // ---- T-07 budget_head_master ----
            // Step 1: DDL. Width guard, then narrow sub_major_acc_code before complete_code (a generated column) depends on it.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_01_head_master_width_guard.sql"));

            migrationBuilder.AlterColumn<string>(
                name: "sub_major_acc_code",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4)",
                oldMaxLength: 4);

            // Identity: PostgreSQL numbers the existing rows (a value the DDL derives, no T-51 row).
            migrationBuilder.AddColumn<long>(
                name: "account_code_id",
                schema: "budget",
                table: "budget_head_master",
                type: "bigint",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                schema: "budget",
                table: "budget_head_master",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                schema: "budget",
                table: "budget_head_master",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_charge_id",
                schema: "budget",
                table: "budget_head_master",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "eoffice_approval_ref",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "not_for_use",
                schema: "budget",
                table: "budget_head_master",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "not_for_use_at",
                schema: "budget",
                table: "budget_head_master",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "not_for_use_by",
                schema: "budget",
                table: "budget_head_master",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "not_for_use_ref",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origin",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true); // filled by the audited backfill, then SET NOT NULL (step 4)

            migrationBuilder.AddColumn<string>(
                name: "complete_code",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(22)",
                maxLength: 22,
                nullable: false,
                computedColumnSql: "major_acc_code || '-' || sub_major_acc_code || '-' || minor_acc_code || '-' || sub_minor_acc_code || '-' || detail_acc_code || '-' || object_acc_code",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_account_code_id",
                schema: "budget",
                table: "budget_head_master",
                column: "account_code_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_complete_code",
                schema: "budget",
                table: "budget_head_master",
                column: "complete_code",
                unique: true);

            // Step 2: actor, audit and DELETE/TRUNCATE triggers on T-07.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_02_head_master_triggers.sql"));

            // Step 3: audited backfills (origin, party_emp_required) under the migration actor.
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName, Gate));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_03_head_master_backfill.sql"));

            // Step 4: NOT NULL (origin, party_emp_required; replaces the generated AlterColumn) and the T-07 CHECKs.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_04_head_master_rules.sql"));

            // ---- T-23 work ----
            // Step 1 (work): DDL.

            migrationBuilder.AlterColumn<string>(
                name: "work_desc",
                schema: "budget",
                table: "work",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                schema: "budget",
                table: "work",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                schema: "budget",
                table: "work",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_charge_id",
                schema: "budget",
                table: "work",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "entered_via",
                schema: "budget",
                table: "work",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "pre_ddd",
                schema: "budget",
                table: "work",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "preceding_amr_work_id",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                schema: "budget",
                table: "work",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by",
                schema: "budget",
                table: "work",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_charge_id",
                schema: "budget",
                table: "work",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "work_class",
                schema: "budget",
                table: "work",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_preceding_amr_work_id",
                schema: "budget",
                table: "work",
                column: "preceding_amr_work_id");

            migrationBuilder.AddForeignKey(
                name: "FK_work_work_preceding_amr_work_id",
                schema: "budget",
                table: "work",
                column: "preceding_amr_work_id",
                principalSchema: "budget",
                principalTable: "work",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        

            // Step 2 (work): actor, audit and DELETE/TRUNCATE triggers.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_05_work_triggers.sql"));

            // Step 3 (work): pre_ddd = true for every existing work, under the migration actor (audited).
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName, Gate));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_06_work_backfill.sql"));

            // Step 4 (work): CHECKs, employee FKs, guarded six-column FK to T-07.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_07_work_rules.sql"));

            // Step 5 (work): trg_work_pre_ddd_one_way, last, because the backfill is a false -> true update.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_08_work_pre_ddd_one_way.sql"));

            // T-01 to T-06: statement-level DELETE/TRUNCATE guard (§11.2).
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpineMasters_09_segment_statement_guards.sql"));
        }

        /// <inheritdoc />
        // Development databases only (design §11.1 rule 6).
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP TRIGGER trg_fa_major_head_forbid_stmt ON budget.fa_major_head; " +
                "DROP TRIGGER trg_fa_sub_major_head_forbid_stmt ON budget.fa_sub_major_head; " +
                "DROP TRIGGER trg_fa_minor_head_forbid_stmt ON budget.fa_minor_head; " +
                "DROP TRIGGER trg_fa_sub_minor_head_forbid_stmt ON budget.fa_sub_minor_head; " +
                "DROP TRIGGER trg_fa_detail_head_forbid_stmt ON budget.fa_detail_head; " +
                "DROP TRIGGER trg_fa_object_head_forbid_stmt ON budget.fa_object_head; " +
                "DROP TRIGGER trg_work_pre_ddd_one_way ON budget.work; DROP FUNCTION budget.fn_work_pre_ddd_one_way(); " +
                "DROP TRIGGER trg_work_require_actor ON budget.work; DROP TRIGGER trg_work_audit ON budget.work; " +
                "DROP TRIGGER trg_work_forbid_stmt ON budget.work; " +
                "ALTER TABLE budget.work DROP CONSTRAINT fk_work_account_code, DROP CONSTRAINT ck_work_class, " +
                "DROP CONSTRAINT ck_work_entered_via, DROP CONSTRAINT ck_work_pre_ddd, DROP CONSTRAINT ck_work_preceding_amr, " +
                "DROP CONSTRAINT ck_work_created_all_or_none, DROP CONSTRAINT fk_work_created_by, DROP CONSTRAINT fk_work_created_charge, " +
                "DROP CONSTRAINT fk_work_updated_by, DROP CONSTRAINT fk_work_updated_charge; " +
                "ALTER TABLE budget.setting DROP CONSTRAINT fk_setting_account_code; " +
                "DROP TRIGGER trg_budget_head_master_require_actor ON budget.budget_head_master; " +
                "DROP TRIGGER trg_budget_head_master_audit ON budget.budget_head_master; " +
                "DROP TRIGGER trg_budget_head_master_forbid_stmt ON budget.budget_head_master; " +
                "ALTER TABLE budget.budget_head_master DROP CONSTRAINT ck_budget_head_master_party_emp, " +
                "DROP CONSTRAINT ck_budget_head_master_origin, DROP CONSTRAINT ck_budget_head_master_eoffice_ref, " +
                "DROP CONSTRAINT ck_budget_head_master_not_for_use, DROP CONSTRAINT ck_budget_head_master_created, " +
                "DROP CONSTRAINT fk_budget_head_master_created_by, DROP CONSTRAINT fk_budget_head_master_created_charge, " +
                "DROP CONSTRAINT fk_budget_head_master_not_for_use_by;");
            migrationBuilder.DropForeignKey(
                name: "FK_work_work_preceding_amr_work_id",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_preceding_amr_work_id",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_account_code_id",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_complete_code",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "complete_code",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "created_charge_id",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "entered_via",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "pre_ddd",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "preceding_amr_work_id",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "updated_at",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "updated_by",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "updated_charge_id",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "work_class",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "account_code_id",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "created_charge_id",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "eoffice_approval_ref",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "not_for_use",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "not_for_use_at",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "not_for_use_by",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "not_for_use_ref",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "origin",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.AlterColumn<string>(
                name: "work_desc",
                schema: "budget",
                table: "work",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "party_emp_required",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "sub_major_acc_code",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(4)",
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2);
        }
    }
}
