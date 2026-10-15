using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <summary>
    /// Design §11.4 P2 row 2: segment masters widened (200; object stays 300) with the DDD code CHECKs and the two
    /// composite T-07 FKs, all behind a guard; row-level UPDATE guard on T-01 to T-06 (insert-only);
    /// trg_head_master_protect (DB-03 marking side and the column-limited update); DB-03 on work's six-column
    /// reference; T-21 with DB-01/DB-02/DB-03 and the T-30 side of DB-02; V-01, V-07; conditional grants.
    /// Additive and writes no data, so it needs neither a gate nor the migration actor (§11.1 rules 8, 9).
    /// Up() is hand-ordered: guard, widen, T-21 DDL, rules, functions, triggers, views, grants (§11.1 rule 2).
    /// </summary>
    public partial class BudgetP2AccountCodesAndAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AccountCodesAndAssignment_01_guard.sql"));
            migrationBuilder.AlterColumn<string>(
                name: "sub_minor_acc_desc",
                schema: "budget",
                table: "fa_sub_minor_head",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "sub_major_acc_desc",
                schema: "budget",
                table: "fa_sub_major_head",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "minor_acc_desc",
                schema: "budget",
                table: "fa_minor_head",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "major_acc_desc",
                schema: "budget",
                table: "fa_major_head",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "detail_acc_desc",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "account_head_assignment",
                schema: "budget",
                columns: table => new
                {
                    assignment_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    office_id = table.Column<long>(type: "bigint", nullable: false),
                    account_code_id = table.Column<long>(type: "bigint", nullable: false),
                    source = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    removed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    removed_by = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_account_head_assignment", x => x.assignment_id);
                    table.ForeignKey(
                        name: "FK_account_head_assignment_office_office_id",
                        column: x => x.office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_head_assignment_account_code_id",
                schema: "budget",
                table: "account_head_assignment",
                column: "account_code_id");

            migrationBuilder.CreateIndex(
                name: "ux_account_head_assignment_live",
                schema: "budget",
                table: "account_head_assignment",
                columns: new[] { "office_id", "account_code_id" },
                unique: true,
                filter: "removed_at IS NULL");
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AccountCodesAndAssignment_02_segment_rules.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AccountCodesAndAssignment_03_functions.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AccountCodesAndAssignment_04_triggers.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AccountCodesAndAssignment_05_views.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AccountCodesAndAssignment_06_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AccountCodesAndAssignment_down.sql"));
            migrationBuilder.DropTable(
                name: "account_head_assignment",
                schema: "budget");

            migrationBuilder.AlterColumn<string>(
                name: "sub_minor_acc_desc",
                schema: "budget",
                table: "fa_sub_minor_head",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "sub_major_acc_desc",
                schema: "budget",
                table: "fa_sub_major_head",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "minor_acc_desc",
                schema: "budget",
                table: "fa_minor_head",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "major_acc_desc",
                schema: "budget",
                table: "fa_major_head",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "detail_acc_desc",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}
