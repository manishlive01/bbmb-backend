using System;
using System.Net;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetP1ReferenceStateAndAudit : Migration
    {
        private const string MigrationName = "BudgetP1ReferenceStateAndAudit";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-ordered to design §11.4 row 2 and §11.1 rules 2, 5, 8 (numbered steps = SQL file numbers).

            // Step 1: EmployeeContext first (rule 5).
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_01_employee_schema_check.sql"));

            // Step 2: migration-actor check before any change: a run without the executing officer aborts here.
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName));

            // Step 3: T-13 office and T-51 audit_log, with the rules EF cannot express.

            migrationBuilder.CreateTable(
                name: "office",
                schema: "budget",
                columns: table => new
                {
                    office_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    office_level = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    division_id = table.Column<int>(type: "integer", nullable: true),
                    circle_id = table.Column<int>(type: "integer", nullable: true),
                    ce_id = table.Column<int>(type: "integer", nullable: true),
                    office_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    wing_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    parent_office_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_charge_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_office", x => x.office_id);
                    table.ForeignKey(
                        name: "FK_office_ce_management_ce_id",
                        column: x => x.ce_id,
                        principalSchema: "budget",
                        principalTable: "ce_management",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_office_cr_management_circle_id",
                        column: x => x.circle_id,
                        principalSchema: "budget",
                        principalTable: "cr_management",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_office_division_division_id",
                        column: x => x.division_id,
                        principalSchema: "budget",
                        principalTable: "division",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_office_irr_pwr_wing_code",
                        column: x => x.wing_code,
                        principalSchema: "budget",
                        principalTable: "irr_pwr",
                        principalColumn: "irr_pwr_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_office_office_parent_office_id",
                        column: x => x.parent_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_office_ce_id",
                schema: "budget",
                table: "office",
                column: "ce_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_office_circle_id",
                schema: "budget",
                table: "office",
                column: "circle_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_office_division_id",
                schema: "budget",
                table: "office",
                column: "division_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_office_parent_office_id",
                schema: "budget",
                table: "office",
                column: "parent_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_office_wing_code",
                schema: "budget",
                table: "office",
                column: "wing_code");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "budget",
                columns: table => new
                {
                    audit_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "clock_timestamp()"),
                    transaction_id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "txid_current()"),
                    table_name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    record_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    operation = table.Column<string>(type: "character(1)", fixedLength: true, maxLength: 1, nullable: false),
                    column_name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: true),
                    old_value = table.Column<string>(type: "jsonb", nullable: true),
                    new_value = table.Column<string>(type: "jsonb", nullable: true),
                    action_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    actor_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actor_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_office_id = table.Column<long>(type: "bigint", nullable: false),
                    actor_role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    on_behalf_of_post = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    client_ip = table.Column<IPAddress>(type: "inet", nullable: true),
                    request_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.audit_id);
                    table.ForeignKey(
                        name: "FK_audit_log_office_actor_office_id",
                        column: x => x.actor_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_actor_office_id_occurred_at",
                schema: "budget",
                table: "audit_log",
                columns: new[] { "actor_office_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_occurred_at",
                schema: "budget",
                table: "audit_log",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_audit_log_table_name_record_key",
                schema: "budget",
                table: "audit_log",
                columns: new[] { "table_name", "record_key" });

            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_03_office_audit_rules.sql"));

            // Step 4: fn_require_actor, fn_audit_row, fn_forbid_change (DDD §6.3, design §4.3).
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_04_functions.sql"));

            // Step 5: IT Cell bootstrap row under the migration actor (rule 8, DDD L-03).
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_05_it_cell_bootstrap.sql"));

            // Step 6: T-14 budget_cycle, T-15 state, T-17 setting.

            migrationBuilder.CreateTable(
                name: "budget_cycle",
                schema: "budget",
                columns: table => new
                {
                    cycle_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    current_fin_year = table.Column<int>(type: "integer", nullable: false),
                    next_fin_year = table.Column<int>(type: "integer", nullable: false),
                    is_first_cycle = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_charge_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_cycle", x => x.cycle_id);
                    table.ForeignKey(
                        name: "FK_budget_cycle_fa_financial_years_current_fin_year",
                        column: x => x.current_fin_year,
                        principalSchema: "budget",
                        principalTable: "fa_financial_years",
                        principalColumn: "fin_year_closing",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_budget_cycle_fa_financial_years_next_fin_year",
                        column: x => x.next_fin_year,
                        principalSchema: "budget",
                        principalTable: "fa_financial_years",
                        principalColumn: "fin_year_closing",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_cycle_current_fin_year",
                schema: "budget",
                table: "budget_cycle",
                column: "current_fin_year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_cycle_is_first_cycle",
                schema: "budget",
                table: "budget_cycle",
                column: "is_first_cycle",
                unique: true,
                filter: "is_first_cycle");

            migrationBuilder.CreateIndex(
                name: "IX_budget_cycle_next_fin_year",
                schema: "budget",
                table: "budget_cycle",
                column: "next_fin_year");

            migrationBuilder.CreateTable(
                name: "state",
                schema: "budget",
                columns: table => new
                {
                    state_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    state_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    holder_level = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    figures_open = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_state", x => x.state_code);
                });

            migrationBuilder.CreateTable(
                name: "setting",
                schema: "budget",
                columns: table => new
                {
                    setting_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    setting_key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    numeric_value = table.Column<decimal>(type: "numeric(15,2)", precision: 15, scale: 2, nullable: true),
                    account_code_id = table.Column<long>(type: "bigint", nullable: true),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    authority_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_setting", x => x.setting_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_setting_setting_key_effective_from",
                schema: "budget",
                table: "setting",
                columns: new[] { "setting_key", "effective_from" },
                unique: true);
        

            // Step 7: their CHECKs, cross-schema FKs and triggers (§11.2).
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_07_cycle_state_setting_rules.sql"));

            // Step 8: T-15 seed, sixteen states (DDD L-02), under the migration actor.
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_08_state_seed.sql"));

            // Step 9: T-17 seed, five keys (DDD L-02, T-17 note), under the migration actor.
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_09_setting_seed.sql"));

            // Step 10: T-08 start/end timestamptz -> date and CHECKs, guarded. Replaces the two generated
            // AlterColumn operations, which would convert in the session time zone without a guard.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_10_financial_year_dates.sql"));

            // Step 11: conditional grants (rule 7, OI-03).
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ReferenceStateAndAudit_11_grants.sql"));
        }

        /// <inheritdoc />
        // Development databases only (design §11.1 rule 6): production rollback is a restore.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE budget.fa_financial_years DROP CONSTRAINT ck_fa_financial_years_closing, " +
                                 "DROP CONSTRAINT ck_fa_financial_years_start, DROP CONSTRAINT ck_fa_financial_years_end;");
            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_cycle",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "setting",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "state",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "office",
                schema: "budget");

            migrationBuilder.AlterColumn<DateTime>(
                name: "start_date",
                schema: "budget",
                table: "fa_financial_years",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "end_date",
                schema: "budget",
                table: "fa_financial_years",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.Sql("DROP FUNCTION budget.fn_forbid_change(); DROP FUNCTION budget.fn_audit_row(); DROP FUNCTION budget.fn_require_actor();");
        }
    }
}
