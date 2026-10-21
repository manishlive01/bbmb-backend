using System;
using System.Net;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetP1ProposalSpine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-ordered to design §11.4 row 3 (PD-1) and §11.1 rules 2 and 9.
            // Step 0: gate guard, the first statement.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpine_00_gate.sql"));

            // Step 1: T-29 to T-33 (EF). T-33 is created without its composite FK to T-16.

            migrationBuilder.CreateTable(
                name: "proposal",
                schema: "budget",
                columns: table => new
                {
                    proposal_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cycle_id = table.Column<long>(type: "bigint", nullable: false),
                    category = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    origin_office_id = table.Column<long>(type: "bigint", nullable: false),
                    state_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    holder_office_id = table.Column<long>(type: "bigint", nullable: false),
                    first_submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    edit_lease_by = table.Column<Guid>(type: "uuid", nullable: true),
                    edit_lease_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_proposal", x => x.proposal_id);
                    table.ForeignKey(
                        name: "FK_proposal_budget_cycle_cycle_id",
                        column: x => x.cycle_id,
                        principalSchema: "budget",
                        principalTable: "budget_cycle",
                        principalColumn: "cycle_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proposal_office_holder_office_id",
                        column: x => x.holder_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proposal_office_origin_office_id",
                        column: x => x.origin_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proposal_state_state_code",
                        column: x => x.state_code,
                        principalSchema: "budget",
                        principalTable: "state",
                        principalColumn: "state_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "demand_line",
                schema: "budget",
                columns: table => new
                {
                    demand_line_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    proposal_id = table.Column<long>(type: "bigint", nullable: false),
                    line_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    serial_no = table.Column<int>(type: "integer", nullable: false),
                    work_id = table.Column<int>(type: "integer", nullable: true),
                    account_code_id = table.Column<long>(type: "bigint", nullable: true),
                    unforeseen_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    rbe_amount = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    obe_next_amount = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    justification_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    justification_attachment_id = table.Column<long>(type: "bigint", nullable: true),
                    ce_certificate_attachment_id = table.Column<long>(type: "bigint", nullable: true),
                    ce_amr_approval_attachment_id = table.Column<long>(type: "bigint", nullable: true),
                    pct_check_not_applied = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    floor_check_unchecked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    added_on_return = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    upload_batch_id = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_demand_line", x => x.demand_line_id);
                    table.ForeignKey(
                        name: "FK_demand_line_proposal_proposal_id",
                        column: x => x.proposal_id,
                        principalSchema: "budget",
                        principalTable: "proposal",
                        principalColumn: "proposal_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_demand_line_work_work_id",
                        column: x => x.work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proposal_movement",
                schema: "budget",
                columns: table => new
                {
                    movement_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    proposal_id = table.Column<long>(type: "bigint", nullable: false),
                    from_state = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    action_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    to_state = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    from_office_id = table.Column<long>(type: "bigint", nullable: false),
                    to_office_id = table.Column<long>(type: "bigint", nullable: false),
                    remark = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    on_behalf_of_post = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    client_ip = table.Column<IPAddress>(type: "inet", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposal_movement", x => x.movement_id);
                    table.ForeignKey(
                        name: "FK_proposal_movement_office_from_office_id",
                        column: x => x.from_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proposal_movement_office_to_office_id",
                        column: x => x.to_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_proposal_movement_proposal_proposal_id",
                        column: x => x.proposal_id,
                        principalSchema: "budget",
                        principalTable: "proposal",
                        principalColumn: "proposal_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "figure_version",
                schema: "budget",
                columns: table => new
                {
                    figure_version_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    demand_line_id = table.Column<long>(type: "bigint", nullable: false),
                    figure = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    value = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    level_office_id = table.Column<long>(type: "bigint", nullable: false),
                    state_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    change_source = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    justification = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    continued_without_justification = table.Column<bool>(type: "boolean", nullable: false),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    on_behalf_of_post = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_figure_version", x => x.figure_version_id);
                    table.ForeignKey(
                        name: "FK_figure_version_demand_line_demand_line_id",
                        column: x => x.demand_line_id,
                        principalSchema: "budget",
                        principalTable: "demand_line",
                        principalColumn: "demand_line_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_figure_version_office_level_office_id",
                        column: x => x.level_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_figure_version_state_state_code",
                        column: x => x.state_code,
                        principalSchema: "budget",
                        principalTable: "state",
                        principalColumn: "state_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "observation",
                schema: "budget",
                columns: table => new
                {
                    observation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    proposal_id = table.Column<long>(type: "bigint", nullable: false),
                    demand_line_id = table.Column<long>(type: "bigint", nullable: true),
                    level_office_id = table.Column<long>(type: "bigint", nullable: false),
                    observation_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    selected_for_return = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    returned_by_movement_id = table.Column<long>(type: "bigint", nullable: true),
                    reply_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    replied_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    replied_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_observation", x => x.observation_id);
                    table.ForeignKey(
                        name: "FK_observation_demand_line_demand_line_id",
                        column: x => x.demand_line_id,
                        principalSchema: "budget",
                        principalTable: "demand_line",
                        principalColumn: "demand_line_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_observation_office_level_office_id",
                        column: x => x.level_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_observation_proposal_movement_returned_by_movement_id",
                        column: x => x.returned_by_movement_id,
                        principalSchema: "budget",
                        principalTable: "proposal_movement",
                        principalColumn: "movement_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_observation_proposal_proposal_id",
                        column: x => x.proposal_id,
                        principalSchema: "budget",
                        principalTable: "proposal",
                        principalColumn: "proposal_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_demand_line_proposal_id_serial_no",
                schema: "budget",
                table: "demand_line",
                columns: new[] { "proposal_id", "serial_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_demand_line_work_id",
                schema: "budget",
                table: "demand_line",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "ux_demand_line_head",
                schema: "budget",
                table: "demand_line",
                columns: new[] { "proposal_id", "account_code_id" },
                unique: true,
                filter: "NOT is_deleted AND line_type = 'HEAD'");

            migrationBuilder.CreateIndex(
                name: "ux_demand_line_work",
                schema: "budget",
                table: "demand_line",
                columns: new[] { "proposal_id", "work_id" },
                unique: true,
                filter: "NOT is_deleted AND line_type = 'WORK'");

            migrationBuilder.CreateIndex(
                name: "IX_figure_version_demand_line_id_figure_created_at",
                schema: "budget",
                table: "figure_version",
                columns: new[] { "demand_line_id", "figure", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_figure_version_level_office_id",
                schema: "budget",
                table: "figure_version",
                column: "level_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_figure_version_state_code",
                schema: "budget",
                table: "figure_version",
                column: "state_code");

            migrationBuilder.CreateIndex(
                name: "IX_observation_demand_line_id",
                schema: "budget",
                table: "observation",
                column: "demand_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_observation_level_office_id",
                schema: "budget",
                table: "observation",
                column: "level_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_observation_proposal_id",
                schema: "budget",
                table: "observation",
                column: "proposal_id");

            migrationBuilder.CreateIndex(
                name: "IX_observation_returned_by_movement_id",
                schema: "budget",
                table: "observation",
                column: "returned_by_movement_id");

            migrationBuilder.CreateIndex(
                name: "IX_proposal_cycle_id_category_origin_office_id",
                schema: "budget",
                table: "proposal",
                columns: new[] { "cycle_id", "category", "origin_office_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proposal_cycle_id_category_state_code",
                schema: "budget",
                table: "proposal",
                columns: new[] { "cycle_id", "category", "state_code" });

            migrationBuilder.CreateIndex(
                name: "IX_proposal_holder_office_id_state_code",
                schema: "budget",
                table: "proposal",
                columns: new[] { "holder_office_id", "state_code" });

            migrationBuilder.CreateIndex(
                name: "IX_proposal_origin_office_id",
                schema: "budget",
                table: "proposal",
                column: "origin_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_proposal_state_code",
                schema: "budget",
                table: "proposal",
                column: "state_code");

            migrationBuilder.CreateIndex(
                name: "IX_proposal_movement_from_office_id",
                schema: "budget",
                table: "proposal_movement",
                column: "from_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_proposal_movement_proposal_id_created_at",
                schema: "budget",
                table: "proposal_movement",
                columns: new[] { "proposal_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_proposal_movement_to_office_id",
                schema: "budget",
                table: "proposal_movement",
                column: "to_office_id");
        
            // Step 2: CHECKs and the employee / T-07 references.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpine_02_rules.sql"));

            // Step 3: DB-01, DB-03 (T-30), DB-04, DB-10 to DB-15 functions, incl. fn_capture_figure_version.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpine_03_functions.sql"));

            // Step 4: trigger attachment (§11.2) and rule triggers.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpine_04_triggers.sql"));

            // Step 5: conditional grants (rule 7, OI-03).
            migrationBuilder.Sql(SqlResource.Read("BudgetP1ProposalSpine_05_grants.sql"));
        }

        /// <inheritdoc />
        // Development databases only (design §11.1 rule 6). Dropping the tables drops their triggers.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "figure_version",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "observation",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "demand_line",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "proposal_movement",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "proposal",
                schema: "budget");

            migrationBuilder.Sql("DROP FUNCTION budget.fn_proposal_insert_rules(); DROP FUNCTION budget.fn_proposal_fixed_columns(); " +
                                 "DROP FUNCTION budget.fn_proposal_state_change(); DROP FUNCTION budget.fn_movement_rules(); " +
                                 "DROP FUNCTION budget.fn_demand_line_rules(); DROP FUNCTION budget.fn_capture_figure_version(); " +
                                 "DROP FUNCTION budget.fn_observation_protect();");
        }
    }
}
