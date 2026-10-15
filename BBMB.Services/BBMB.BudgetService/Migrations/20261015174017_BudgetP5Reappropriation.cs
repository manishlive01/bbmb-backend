using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    // Design §11.4 P5 row: T-46, T-47 (+ is_removed, §5.5 row c); DB-21 (the T-42 reappropriation_id FK),
    // DB-24; T-45 cleared_by_reappropriation_id; V-10. GATED on bbmb_gate.p5_ddd_deviations (CR-DDD-03). The
    // migration writes no data (no migration actor); the EF DDL stands first, then the hand SQL
    // (gate -> rules -> functions -> triggers -> views -> grants).
    public partial class BudgetP5Reappropriation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // §11.1 rule 9: the gate guard runs first; the migration aborts unless the decision reference is set.
            migrationBuilder.Sql(SqlResource.Read("BudgetP5Reappropriation_00_gate.sql"));

            migrationBuilder.AddColumn<long>(
                name: "cleared_by_reappropriation_id",
                schema: "budget",
                table: "correction_flag",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reappropriation",
                schema: "budget",
                columns: table => new
                {
                    reappropriation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fin_year = table.Column<int>(type: "integer", nullable: false),
                    serial_no = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    destination_provision_id = table.Column<long>(type: "bigint", nullable: true),
                    destination_new_work_id = table.Column<int>(type: "integer", nullable: true),
                    additional_amount = table.Column<decimal>(type: "budget.rupees", nullable: false),
                    eoffice_order_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    remarks = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    order_attachment_id = table.Column<long>(type: "bigint", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_by = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_reappropriation", x => x.reappropriation_id);
                    table.ForeignKey(
                        name: "FK_reappropriation_attachment_order_attachment_id",
                        column: x => x.order_attachment_id,
                        principalSchema: "budget",
                        principalTable: "attachment",
                        principalColumn: "attachment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reappropriation_fa_financial_years_fin_year",
                        column: x => x.fin_year,
                        principalSchema: "budget",
                        principalTable: "fa_financial_years",
                        principalColumn: "fin_year_closing",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reappropriation_provision_destination_provision_id",
                        column: x => x.destination_provision_id,
                        principalSchema: "budget",
                        principalTable: "provision",
                        principalColumn: "provision_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reappropriation_work_destination_new_work_id",
                        column: x => x.destination_new_work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reappropriation_source",
                schema: "budget",
                columns: table => new
                {
                    reappropriation_source_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    reappropriation_id = table.Column<long>(type: "bigint", nullable: false),
                    provision_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "budget.rupees", nullable: false),
                    is_removed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_reappropriation_source", x => x.reappropriation_source_id);
                    table.ForeignKey(
                        name: "FK_reappropriation_source_provision_provision_id",
                        column: x => x.provision_id,
                        principalSchema: "budget",
                        principalTable: "provision",
                        principalColumn: "provision_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reappropriation_source_reappropriation_reappropriation_id",
                        column: x => x.reappropriation_id,
                        principalSchema: "budget",
                        principalTable: "reappropriation",
                        principalColumn: "reappropriation_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_provision_movement_reappropriation_id",
                schema: "budget",
                table: "provision_movement",
                column: "reappropriation_id");

            migrationBuilder.CreateIndex(
                name: "IX_correction_flag_cleared_by_reappropriation_id",
                schema: "budget",
                table: "correction_flag",
                column: "cleared_by_reappropriation_id");

            migrationBuilder.CreateIndex(
                name: "IX_reappropriation_destination_new_work_id",
                schema: "budget",
                table: "reappropriation",
                column: "destination_new_work_id");

            migrationBuilder.CreateIndex(
                name: "IX_reappropriation_destination_provision_id",
                schema: "budget",
                table: "reappropriation",
                column: "destination_provision_id");

            migrationBuilder.CreateIndex(
                name: "IX_reappropriation_fin_year_serial_no",
                schema: "budget",
                table: "reappropriation",
                columns: new[] { "fin_year", "serial_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reappropriation_order_attachment_id",
                schema: "budget",
                table: "reappropriation",
                column: "order_attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_reappropriation_source_provision_id",
                schema: "budget",
                table: "reappropriation_source",
                column: "provision_id");

            migrationBuilder.CreateIndex(
                name: "IX_reappropriation_source_reappropriation_id_provision_id",
                schema: "budget",
                table: "reappropriation_source",
                columns: new[] { "reappropriation_id", "provision_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_correction_flag_reappropriation_cleared_by_reappropriation_~",
                schema: "budget",
                table: "correction_flag",
                column: "cleared_by_reappropriation_id",
                principalSchema: "budget",
                principalTable: "reappropriation",
                principalColumn: "reappropriation_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_provision_movement_reappropriation_reappropriation_id",
                schema: "budget",
                table: "provision_movement",
                column: "reappropriation_id",
                principalSchema: "budget",
                principalTable: "reappropriation",
                principalColumn: "reappropriation_id",
                onDelete: ReferentialAction.Restrict);

            // Hand SQL (design §11.1 rule 2), after the EF DDL: DDD CHECKs + cross-schema actor FKs + the
            // partial UK (created_by) where OPEN; the DB-24 / VR-08 functions; trigger attachment (§11.2); V-10;
            // conditional grants (§11.1 rule 7).
            migrationBuilder.Sql(SqlResource.Read("BudgetP5Reappropriation_02_rules.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP5Reappropriation_03_functions.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP5Reappropriation_04_triggers.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP5Reappropriation_05_views.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP5Reappropriation_06_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Undo the hand SQL first: drop V-10 and the standalone functions, so the EF DropTable/DropColumn
            // below do not fail on a dependent object.
            migrationBuilder.Sql(SqlResource.Read("BudgetP5Reappropriation_down.sql"));

            migrationBuilder.DropForeignKey(
                name: "FK_correction_flag_reappropriation_cleared_by_reappropriation_~",
                schema: "budget",
                table: "correction_flag");

            migrationBuilder.DropForeignKey(
                name: "FK_provision_movement_reappropriation_reappropriation_id",
                schema: "budget",
                table: "provision_movement");

            migrationBuilder.DropTable(
                name: "reappropriation_source",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "reappropriation",
                schema: "budget");

            migrationBuilder.DropIndex(
                name: "IX_provision_movement_reappropriation_id",
                schema: "budget",
                table: "provision_movement");

            migrationBuilder.DropIndex(
                name: "IX_correction_flag_cleared_by_reappropriation_id",
                schema: "budget",
                table: "correction_flag");

            migrationBuilder.DropColumn(
                name: "cleared_by_reappropriation_id",
                schema: "budget",
                table: "correction_flag");
        }
    }
}
