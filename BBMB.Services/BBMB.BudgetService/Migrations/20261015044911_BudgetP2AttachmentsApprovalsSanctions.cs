using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetP2AttachmentsApprovalsSanctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-ordered (design §11.1 rule 2). Not gated: no DDD deviation awaiting a decision (design §11.4).
            // Step 1: generated DDL (T-22, T-19, T-25 to T-28, T-45; T-30 attachment FKs). Steps 2 to 6 after it.
            migrationBuilder.CreateTable(
                name: "admin_approval",
                schema: "budget",
                columns: table => new
                {
                    admin_approval_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    work_id = table.Column<int>(type: "integer", nullable: false),
                    division_office_id = table.Column<long>(type: "bigint", nullable: false),
                    serial_no = table.Column<int>(type: "integer", nullable: false),
                    current_version_no = table.Column<short>(type: "smallint", nullable: false),
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
                    table.PrimaryKey("PK_admin_approval", x => x.admin_approval_id);
                    table.ForeignKey(
                        name: "FK_admin_approval_office_division_office_id",
                        column: x => x.division_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admin_approval_work_work_id",
                        column: x => x.work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "attachment",
                schema: "budget",
                columns: table => new
                {
                    attachment_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    storage_key = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_extension = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    evidence_class = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256_digest = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    scan_engine = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    scanned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachment", x => x.attachment_id);
                });

            migrationBuilder.CreateTable(
                name: "correction_flag",
                schema: "budget",
                columns: table => new
                {
                    correction_flag_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    reason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    work_id = table.Column<int>(type: "integer", nullable: true),
                    figure_allocated = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    figure_supported = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    is_critical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    cleared_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cleared_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_correction_flag", x => x.correction_flag_id);
                    table.ForeignKey(
                        name: "FK_correction_flag_work_work_id",
                        column: x => x.work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "delegation_limit",
                schema: "budget",
                columns: table => new
                {
                    delegation_limit_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    authority_level = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    max_amount = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    source_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delegation_limit", x => x.delegation_limit_id);
                });

            migrationBuilder.CreateTable(
                name: "technical_sanction",
                schema: "budget",
                columns: table => new
                {
                    technical_sanction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    work_id = table.Column<int>(type: "integer", nullable: false),
                    tab = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    admin_approval_id = table.Column<long>(type: "bigint", nullable: true),
                    serial_no = table.Column<int>(type: "integer", nullable: false),
                    current_version_no = table.Column<short>(type: "smallint", nullable: false),
                    estimate_code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    estimate_code_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_technical_sanction", x => x.technical_sanction_id);
                    table.ForeignKey(
                        name: "FK_technical_sanction_admin_approval_admin_approval_id",
                        column: x => x.admin_approval_id,
                        principalSchema: "budget",
                        principalTable: "admin_approval",
                        principalColumn: "admin_approval_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_technical_sanction_work_work_id",
                        column: x => x.work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "admin_approval_version",
                schema: "budget",
                columns: table => new
                {
                    admin_approval_id = table.Column<long>(type: "bigint", nullable: false),
                    version_no = table.Column<short>(type: "smallint", nullable: false),
                    name_of_work = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    aa_reference_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    date_of_approval = table.Column<DateOnly>(type: "date", nullable: false),
                    account_code_id = table.Column<long>(type: "bigint", nullable: false),
                    amount_sanctioned = table.Column<decimal>(type: "budget.rupees", nullable: false),
                    aa_copy_attachment_id = table.Column<long>(type: "bigint", nullable: false),
                    eoffice_ref = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    competent_authority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    exceeds_delegation = table.Column<bool>(type: "boolean", nullable: false),
                    delegation_justification = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_approval_version", x => new { x.admin_approval_id, x.version_no });
                    table.ForeignKey(
                        name: "FK_admin_approval_version_admin_approval_admin_approval_id",
                        column: x => x.admin_approval_id,
                        principalSchema: "budget",
                        principalTable: "admin_approval",
                        principalColumn: "admin_approval_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_admin_approval_version_attachment_aa_copy_attachment_id",
                        column: x => x.aa_copy_attachment_id,
                        principalSchema: "budget",
                        principalTable: "attachment",
                        principalColumn: "attachment_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "technical_sanction_version",
                schema: "budget",
                columns: table => new
                {
                    technical_sanction_id = table.Column<long>(type: "bigint", nullable: false),
                    version_no = table.Column<short>(type: "smallint", nullable: false),
                    name_of_estimate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ts_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ts_date = table.Column<DateOnly>(type: "date", nullable: false),
                    ts_amount = table.Column<decimal>(type: "budget.rupees", nullable: false),
                    ts_copy_attachment_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technical_sanction_version", x => new { x.technical_sanction_id, x.version_no });
                    table.ForeignKey(
                        name: "FK_technical_sanction_version_attachment_ts_copy_attachment_id",
                        column: x => x.ts_copy_attachment_id,
                        principalSchema: "budget",
                        principalTable: "attachment",
                        principalColumn: "attachment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_technical_sanction_version_technical_sanction_technical_san~",
                        column: x => x.technical_sanction_id,
                        principalSchema: "budget",
                        principalTable: "technical_sanction",
                        principalColumn: "technical_sanction_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_demand_line_ce_amr_approval_attachment_id",
                schema: "budget",
                table: "demand_line",
                column: "ce_amr_approval_attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_demand_line_ce_certificate_attachment_id",
                schema: "budget",
                table: "demand_line",
                column: "ce_certificate_attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_demand_line_justification_attachment_id",
                schema: "budget",
                table: "demand_line",
                column: "justification_attachment_id");

            migrationBuilder.CreateIndex(
                name: "ux_admin_approval_division_serial",
                schema: "budget",
                table: "admin_approval",
                columns: new[] { "division_office_id", "serial_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_admin_approval_work",
                schema: "budget",
                table: "admin_approval",
                column: "work_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_approval_version_aa_copy_attachment_id",
                schema: "budget",
                table: "admin_approval_version",
                column: "aa_copy_attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_attachment_storage_key",
                schema: "budget",
                table: "attachment",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_correction_flag_work_id",
                schema: "budget",
                table: "correction_flag",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_delegation_limit_authority_level_effective_from",
                schema: "budget",
                table: "delegation_limit",
                columns: new[] { "authority_level", "effective_from" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_technical_sanction_admin_approval_id",
                schema: "budget",
                table: "technical_sanction",
                column: "admin_approval_id");

            migrationBuilder.CreateIndex(
                name: "IX_technical_sanction_work_id",
                schema: "budget",
                table: "technical_sanction",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "ux_technical_sanction_amr_work",
                schema: "budget",
                table: "technical_sanction",
                column: "work_id",
                unique: true,
                filter: "tab = 'AMR'");

            migrationBuilder.CreateIndex(
                name: "ux_technical_sanction_estimate_code",
                schema: "budget",
                table: "technical_sanction",
                column: "estimate_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_technical_sanction_version_ts_copy_attachment_id",
                schema: "budget",
                table: "technical_sanction_version",
                column: "ts_copy_attachment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_demand_line_attachment_ce_amr_approval_attachment_id",
                schema: "budget",
                table: "demand_line",
                column: "ce_amr_approval_attachment_id",
                principalSchema: "budget",
                principalTable: "attachment",
                principalColumn: "attachment_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_demand_line_attachment_ce_certificate_attachment_id",
                schema: "budget",
                table: "demand_line",
                column: "ce_certificate_attachment_id",
                principalSchema: "budget",
                principalTable: "attachment",
                principalColumn: "attachment_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_demand_line_attachment_justification_attachment_id",
                schema: "budget",
                table: "demand_line",
                column: "justification_attachment_id",
                principalSchema: "budget",
                principalTable: "attachment",
                principalColumn: "attachment_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(SqlResource.Read("BudgetP2AttachmentsApprovalsSanctions_02_rules.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AttachmentsApprovalsSanctions_03_functions.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AttachmentsApprovalsSanctions_04_triggers.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AttachmentsApprovalsSanctions_05_views.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2AttachmentsApprovalsSanctions_06_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Development databases only (design §11.1 rule 6). V-04 goes back to version 1 (a view cannot drop
            // columns by CREATE OR REPLACE), then the T-30 trigger, then the tables, then the functions.
            migrationBuilder.Sql("DROP VIEW budget.v_fa_work_sanction; DROP VIEW budget.v_demand_line_display;");
            migrationBuilder.Sql(V04Version1());
            migrationBuilder.Sql("DROP TRIGGER trg_demand_line_attachment_rules ON budget.demand_line;");
            migrationBuilder.DropForeignKey(
                name: "FK_demand_line_attachment_ce_amr_approval_attachment_id",
                schema: "budget",
                table: "demand_line");

            migrationBuilder.DropForeignKey(
                name: "FK_demand_line_attachment_ce_certificate_attachment_id",
                schema: "budget",
                table: "demand_line");

            migrationBuilder.DropForeignKey(
                name: "FK_demand_line_attachment_justification_attachment_id",
                schema: "budget",
                table: "demand_line");

            migrationBuilder.DropTable(
                name: "admin_approval_version",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "correction_flag",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "delegation_limit",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "technical_sanction_version",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "attachment",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "technical_sanction",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "admin_approval",
                schema: "budget");

            migrationBuilder.DropIndex(
                name: "IX_demand_line_ce_amr_approval_attachment_id",
                schema: "budget",
                table: "demand_line");

            migrationBuilder.DropIndex(
                name: "IX_demand_line_ce_certificate_attachment_id",
                schema: "budget",
                table: "demand_line");

            migrationBuilder.DropIndex(
                name: "IX_demand_line_justification_attachment_id",
                schema: "budget",
                table: "demand_line");

            migrationBuilder.Sql(
                "DROP FUNCTION budget.fn_record_estimate_code(bigint, varchar); " +
                "DROP FUNCTION budget.fn_demand_line_attachment_rules(); DROP FUNCTION budget.fn_correction_flag_rules(); " +
                "DROP FUNCTION budget.fn_ts_ceiling_check(); DROP FUNCTION budget.fn_technical_sanction_version_check(); " +
                "DROP FUNCTION budget.fn_technical_sanction_version_rules(); DROP FUNCTION budget.fn_technical_sanction_rules(); " +
                "DROP FUNCTION budget.fn_admin_approval_version_check(); DROP FUNCTION budget.fn_admin_approval_version_rules(); " +
                "DROP FUNCTION budget.fn_admin_approval_rules(); DROP FUNCTION budget.fn_require_evidence(bigint, text);");
        }

        /// <summary>The V-04 version 1 statement of BudgetP1FiguresAndRecommendations step 5.</summary>
        private static string V04Version1()
        {
            var sql = SqlResource.Read("BudgetP1FiguresAndRecommendations_05_views.sql");
            var start = sql.IndexOf("CREATE VIEW budget.v_demand_line_display", System.StringComparison.Ordinal);
            return sql[start..];
        }
    }
}
