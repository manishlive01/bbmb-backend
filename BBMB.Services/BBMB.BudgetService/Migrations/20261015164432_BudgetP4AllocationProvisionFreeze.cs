using System;
using System.Net;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    // Design §11.4 P4 row 2: T-40..T-44, T-48..T-50; T-37 minutes_allocation_id + DB-17; T-45 provision_id;
    // DB-19 to DB-23, DB-25, DB-26, DB-21; V-02, V-09, V-04 v4. Ungated and writes no data (only
    // BudgetP4WorkCodeConformance carries work_code_reset, §11.4), so there is no gate guard and no migration
    // actor; the EF-generated DDL stands first, then the hand SQL (rules -> functions -> triggers -> views ->
    // grants), as the P3 migration does.
    public partial class BudgetP4AllocationProvisionFreeze : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "minutes_allocation_id",
                schema: "budget",
                table: "recommendation_version",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "provision_id",
                schema: "budget",
                table: "correction_flag",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "minutes_allocation_id",
                schema: "budget",
                table: "ce_recommendation",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "allocation",
                schema: "budget",
                columns: table => new
                {
                    allocation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cycle_id = table.Column<long>(type: "bigint", nullable: false),
                    body = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    minutes_reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    minutes_date = table.Column<DateOnly>(type: "date", nullable: false),
                    minutes_attachment_id = table.Column<long>(type: "bigint", nullable: false),
                    fa_cao_approval_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    remarks = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    lines_activated = table.Column<int>(type: "integer", nullable: false),
                    total_activated = table.Column<decimal>(type: "budget.rupees", nullable: false),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    on_behalf_of_post = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allocation", x => x.allocation_id);
                    table.ForeignKey(
                        name: "FK_allocation_attachment_minutes_attachment_id",
                        column: x => x.minutes_attachment_id,
                        principalSchema: "budget",
                        principalTable: "attachment",
                        principalColumn: "attachment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_allocation_budget_cycle_cycle_id",
                        column: x => x.cycle_id,
                        principalSchema: "budget",
                        principalTable: "budget_cycle",
                        principalColumn: "cycle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "freeze",
                schema: "budget",
                columns: table => new
                {
                    cycle_id = table.Column<long>(type: "bigint", nullable: false),
                    fa_cao_approval_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    remarks = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    on_behalf_of_post = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_freeze", x => x.cycle_id);
                    table.ForeignKey(
                        name: "FK_freeze_budget_cycle_cycle_id",
                        column: x => x.cycle_id,
                        principalSchema: "budget",
                        principalTable: "budget_cycle",
                        principalColumn: "cycle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provision",
                schema: "budget",
                columns: table => new
                {
                    provision_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fin_year = table.Column<int>(type: "integer", nullable: false),
                    basis = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    category = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    holder_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    work_id = table.Column<int>(type: "integer", nullable: true),
                    office_id = table.Column<long>(type: "bigint", nullable: false),
                    account_code_id = table.Column<long>(type: "bigint", nullable: true),
                    demand_line_id = table.Column<long>(type: "bigint", nullable: true),
                    recommendation_id = table.Column<long>(type: "bigint", nullable: true),
                    distribution_id = table.Column<long>(type: "bigint", nullable: true),
                    allocation_id = table.Column<long>(type: "bigint", nullable: true),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    budgetary_check = table.Column<bool>(type: "boolean", nullable: false),
                    superseded_by_provision_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provision", x => x.provision_id);
                    table.ForeignKey(
                        name: "FK_provision_allocation_allocation_id",
                        column: x => x.allocation_id,
                        principalSchema: "budget",
                        principalTable: "allocation",
                        principalColumn: "allocation_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_ce_recommendation_recommendation_id",
                        column: x => x.recommendation_id,
                        principalSchema: "budget",
                        principalTable: "ce_recommendation",
                        principalColumn: "recommendation_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_demand_line_demand_line_id",
                        column: x => x.demand_line_id,
                        principalSchema: "budget",
                        principalTable: "demand_line",
                        principalColumn: "demand_line_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_distribution_distribution_id",
                        column: x => x.distribution_id,
                        principalSchema: "budget",
                        principalTable: "distribution",
                        principalColumn: "distribution_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_fa_financial_years_fin_year",
                        column: x => x.fin_year,
                        principalSchema: "budget",
                        principalTable: "fa_financial_years",
                        principalColumn: "fin_year_closing",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_office_office_id",
                        column: x => x.office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_provision_superseded_by_provision_id",
                        column: x => x.superseded_by_provision_id,
                        principalSchema: "budget",
                        principalTable: "provision",
                        principalColumn: "provision_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_work_work_id",
                        column: x => x.work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "estimate_allocation",
                schema: "budget",
                columns: table => new
                {
                    estimate_allocation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    provision_id = table.Column<long>(type: "bigint", nullable: false),
                    technical_sanction_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "budget.rupees", nullable: false),
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
                    table.PrimaryKey("PK_estimate_allocation", x => x.estimate_allocation_id);
                    table.ForeignKey(
                        name: "FK_estimate_allocation_provision_provision_id",
                        column: x => x.provision_id,
                        principalSchema: "budget",
                        principalTable: "provision",
                        principalColumn: "provision_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_estimate_allocation_technical_sanction_technical_sanction_id",
                        column: x => x.technical_sanction_id,
                        principalSchema: "budget",
                        principalTable: "technical_sanction",
                        principalColumn: "technical_sanction_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unforeseen_request",
                schema: "budget",
                columns: table => new
                {
                    unforeseen_request_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    work_id = table.Column<int>(type: "integer", nullable: false),
                    division_office_id = table.Column<long>(type: "bigint", nullable: false),
                    current_provision_id = table.Column<long>(type: "bigint", nullable: true),
                    amount_current = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    next_provision_id = table.Column<long>(type: "bigint", nullable: true),
                    amount_next = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    holder_office_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_unforeseen_request", x => x.unforeseen_request_id);
                    table.ForeignKey(
                        name: "FK_unforeseen_request_office_division_office_id",
                        column: x => x.division_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unforeseen_request_office_holder_office_id",
                        column: x => x.holder_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unforeseen_request_provision_current_provision_id",
                        column: x => x.current_provision_id,
                        principalSchema: "budget",
                        principalTable: "provision",
                        principalColumn: "provision_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unforeseen_request_provision_next_provision_id",
                        column: x => x.next_provision_id,
                        principalSchema: "budget",
                        principalTable: "provision",
                        principalColumn: "provision_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unforeseen_request_work_work_id",
                        column: x => x.work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unforeseen_authorisation",
                schema: "budget",
                columns: table => new
                {
                    unforeseen_authorisation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    unforeseen_request_id = table.Column<long>(type: "bigint", nullable: false),
                    ce_office_id = table.Column<long>(type: "bigint", nullable: false),
                    amount_current = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    amount_next = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    reasons = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    request_snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    work_code_generated = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    client_ip = table.Column<IPAddress>(type: "inet", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unforeseen_authorisation", x => x.unforeseen_authorisation_id);
                    table.ForeignKey(
                        name: "FK_unforeseen_authorisation_office_ce_office_id",
                        column: x => x.ce_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unforeseen_authorisation_unforeseen_request_unforeseen_requ~",
                        column: x => x.unforeseen_request_id,
                        principalSchema: "budget",
                        principalTable: "unforeseen_request",
                        principalColumn: "unforeseen_request_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unforeseen_request_movement",
                schema: "budget",
                columns: table => new
                {
                    movement_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    unforeseen_request_id = table.Column<long>(type: "bigint", nullable: false),
                    from_status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    to_status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    from_office_id = table.Column<long>(type: "bigint", nullable: false),
                    to_office_id = table.Column<long>(type: "bigint", nullable: false),
                    remark = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    role_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    client_ip = table.Column<IPAddress>(type: "inet", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unforeseen_request_movement", x => x.movement_id);
                    table.ForeignKey(
                        name: "FK_unforeseen_request_movement_office_from_office_id",
                        column: x => x.from_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unforeseen_request_movement_office_to_office_id",
                        column: x => x.to_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_unforeseen_request_movement_unforeseen_request_unforeseen_r~",
                        column: x => x.unforeseen_request_id,
                        principalSchema: "budget",
                        principalTable: "unforeseen_request",
                        principalColumn: "unforeseen_request_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "provision_movement",
                schema: "budget",
                columns: table => new
                {
                    provision_movement_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    provision_id = table.Column<long>(type: "bigint", nullable: false),
                    movement_type = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    amount = table.Column<decimal>(type: "budget.rupees", nullable: false),
                    allocation_id = table.Column<long>(type: "bigint", nullable: true),
                    reappropriation_id = table.Column<long>(type: "bigint", nullable: true),
                    unforeseen_authorisation_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provision_movement", x => x.provision_movement_id);
                    table.ForeignKey(
                        name: "FK_provision_movement_allocation_allocation_id",
                        column: x => x.allocation_id,
                        principalSchema: "budget",
                        principalTable: "allocation",
                        principalColumn: "allocation_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_movement_provision_provision_id",
                        column: x => x.provision_id,
                        principalSchema: "budget",
                        principalTable: "provision",
                        principalColumn: "provision_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_provision_movement_unforeseen_authorisation_unforeseen_auth~",
                        column: x => x.unforeseen_authorisation_id,
                        principalSchema: "budget",
                        principalTable: "unforeseen_authorisation",
                        principalColumn: "unforeseen_authorisation_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_correction_flag_provision_id",
                schema: "budget",
                table: "correction_flag",
                column: "provision_id");

            migrationBuilder.CreateIndex(
                name: "IX_ce_recommendation_minutes_allocation_id",
                schema: "budget",
                table: "ce_recommendation",
                column: "minutes_allocation_id");

            migrationBuilder.CreateIndex(
                name: "IX_allocation_cycle_id_body",
                schema: "budget",
                table: "allocation",
                columns: new[] { "cycle_id", "body" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_allocation_minutes_attachment_id",
                schema: "budget",
                table: "allocation",
                column: "minutes_attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_estimate_allocation_provision_id_technical_sanction_id",
                schema: "budget",
                table: "estimate_allocation",
                columns: new[] { "provision_id", "technical_sanction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estimate_allocation_technical_sanction_id",
                schema: "budget",
                table: "estimate_allocation",
                column: "technical_sanction_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_allocation_id",
                schema: "budget",
                table: "provision",
                column: "allocation_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_demand_line_id",
                schema: "budget",
                table: "provision",
                column: "demand_line_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_distribution_id",
                schema: "budget",
                table: "provision",
                column: "distribution_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_fin_year_basis_holder_type_work_id_office_id",
                schema: "budget",
                table: "provision",
                columns: new[] { "fin_year", "basis", "holder_type", "work_id", "office_id" });

            migrationBuilder.CreateIndex(
                name: "IX_provision_office_id",
                schema: "budget",
                table: "provision",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_recommendation_id",
                schema: "budget",
                table: "provision",
                column: "recommendation_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_superseded_by_provision_id",
                schema: "budget",
                table: "provision",
                column: "superseded_by_provision_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_work_id",
                schema: "budget",
                table: "provision",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_movement_allocation_id",
                schema: "budget",
                table: "provision_movement",
                column: "allocation_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_movement_provision_id",
                schema: "budget",
                table: "provision_movement",
                column: "provision_id");

            migrationBuilder.CreateIndex(
                name: "IX_provision_movement_unforeseen_authorisation_id",
                schema: "budget",
                table: "provision_movement",
                column: "unforeseen_authorisation_id");

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_authorisation_ce_office_id",
                schema: "budget",
                table: "unforeseen_authorisation",
                column: "ce_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_authorisation_unforeseen_request_id",
                schema: "budget",
                table: "unforeseen_authorisation",
                column: "unforeseen_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_request_current_provision_id",
                schema: "budget",
                table: "unforeseen_request",
                column: "current_provision_id");

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_request_division_office_id",
                schema: "budget",
                table: "unforeseen_request",
                column: "division_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_request_holder_office_id",
                schema: "budget",
                table: "unforeseen_request",
                column: "holder_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_request_next_provision_id",
                schema: "budget",
                table: "unforeseen_request",
                column: "next_provision_id");

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_request_work_id",
                schema: "budget",
                table: "unforeseen_request",
                column: "work_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_request_movement_from_office_id",
                schema: "budget",
                table: "unforeseen_request_movement",
                column: "from_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_request_movement_to_office_id",
                schema: "budget",
                table: "unforeseen_request_movement",
                column: "to_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_unforeseen_request_movement_unforeseen_request_id",
                schema: "budget",
                table: "unforeseen_request_movement",
                column: "unforeseen_request_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ce_recommendation_allocation_minutes_allocation_id",
                schema: "budget",
                table: "ce_recommendation",
                column: "minutes_allocation_id",
                principalSchema: "budget",
                principalTable: "allocation",
                principalColumn: "allocation_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_correction_flag_provision_provision_id",
                schema: "budget",
                table: "correction_flag",
                column: "provision_id",
                principalSchema: "budget",
                principalTable: "provision",
                principalColumn: "provision_id",
                onDelete: ReferentialAction.Restrict);

            // Hand SQL (design §11.1 rule 2), after the EF DDL: DDD CHECKs + cross-schema actor FKs + the
            // live-holder partial unique index; the DB-05/17/19/21/22/23/25/26 rule functions; trigger
            // attachment (§11.2); V-02, V-09 and V-04 v4; conditional grants (§11.1 rule 7).
            migrationBuilder.Sql(SqlResource.Read("BudgetP4AllocationProvisionFreeze_02_rules.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP4AllocationProvisionFreeze_03_functions.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP4AllocationProvisionFreeze_04_triggers.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP4AllocationProvisionFreeze_05_views.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP4AllocationProvisionFreeze_06_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Undo the hand SQL first: drop V-09/V-02, restore V-04 to v3, drop the DB-17 trigger and the
            // standalone functions, so the EF DropTable/DropColumn below do not fail on a dependent object.
            migrationBuilder.Sql(SqlResource.Read("BudgetP4AllocationProvisionFreeze_down.sql"));

            migrationBuilder.DropForeignKey(
                name: "FK_ce_recommendation_allocation_minutes_allocation_id",
                schema: "budget",
                table: "ce_recommendation");

            migrationBuilder.DropForeignKey(
                name: "FK_correction_flag_provision_provision_id",
                schema: "budget",
                table: "correction_flag");

            migrationBuilder.DropTable(
                name: "estimate_allocation",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "freeze",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "provision_movement",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "unforeseen_request_movement",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "unforeseen_authorisation",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "unforeseen_request",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "provision",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "allocation",
                schema: "budget");

            migrationBuilder.DropIndex(
                name: "IX_correction_flag_provision_id",
                schema: "budget",
                table: "correction_flag");

            migrationBuilder.DropIndex(
                name: "IX_ce_recommendation_minutes_allocation_id",
                schema: "budget",
                table: "ce_recommendation");

            migrationBuilder.DropColumn(
                name: "minutes_allocation_id",
                schema: "budget",
                table: "recommendation_version");

            migrationBuilder.DropColumn(
                name: "provision_id",
                schema: "budget",
                table: "correction_flag");

            migrationBuilder.DropColumn(
                name: "minutes_allocation_id",
                schema: "budget",
                table: "ce_recommendation");
        }
    }
}
