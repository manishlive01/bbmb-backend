using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetAuthzConfigTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "budget_project_settings",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    sso_project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sso_project_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    employee_charge_project_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    edit_lease_minutes = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_budget_project_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "budget_query_field",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_budget_query_field", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "budget_right",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    right_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_budget_right", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "budget_role",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    office_level = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_budget_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "budget_admin_role_assignment",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "PENDING"),
                    document_ref = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: true),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoked_by = table.Column<Guid>(type: "uuid", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revoke_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_budget_admin_role_assignment", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_admin_role_assignment_budget_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "budget",
                        principalTable: "budget_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "budget_role_query_field",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    query_field_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_budget_role_query_field", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_role_query_field_budget_query_field_query_field_id",
                        column: x => x.query_field_id,
                        principalSchema: "budget",
                        principalTable: "budget_query_field",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_budget_role_query_field_budget_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "budget",
                        principalTable: "budget_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "budget_role_right",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    right_id = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_budget_role_right", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_role_right_budget_right_right_id",
                        column: x => x.right_id,
                        principalSchema: "budget",
                        principalTable: "budget_right",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_budget_role_right_budget_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "budget",
                        principalTable: "budget_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_admin_role_assignment_role_id",
                schema: "budget",
                table: "budget_admin_role_assignment",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ux_query_field_name",
                schema: "budget",
                table: "budget_query_field",
                column: "field_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_budget_right_key",
                schema: "budget",
                table: "budget_right",
                column: "right_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_budget_role_name",
                schema: "budget",
                table: "budget_role",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_role_query_field_query_field_id",
                schema: "budget",
                table: "budget_role_query_field",
                column: "query_field_id");

            migrationBuilder.CreateIndex(
                name: "ux_role_query_field",
                schema: "budget",
                table: "budget_role_query_field",
                columns: new[] { "role_id", "query_field_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_role_right_right_id",
                schema: "budget",
                table: "budget_role_right",
                column: "right_id");

            migrationBuilder.CreateIndex(
                name: "ux_role_right",
                schema: "budget",
                table: "budget_role_right",
                columns: new[] { "role_id", "right_id" },
                unique: true);

            // Hand SQL, ordered as the P1 examples (design §1, §7). The EF CreateTable above gave the tables,
            // PKs, FKs and the plain/composite UKs; the partial UKs, CHECKs, the cross-schema employee FKs, the
            // actor/audit/forbid triggers, the version-stamp function and the appsettings seed follow here.

            // Step 1: partial UKs, CHECKs and the employee FKs on uploaded_by / approved_by.
            migrationBuilder.Sql(SqlResource.Read("BudgetAuthzConfigTables_01_rules.sql"));

            // Step 2: actor, audit and DELETE/TRUNCATE-forbid triggers on the seven tables, BEFORE any seed
            // insert — exactly as the IT-cell bootstrap attaches triggers before its insert, so every seed row
            // is itself audited into budget.audit_log.
            migrationBuilder.Sql(SqlResource.Read("BudgetAuthzConfigTables_02_triggers.sql"));

            // Step 3: the config version-stamp function (design §2, OQ-1).
            migrationBuilder.Sql(SqlResource.Read("BudgetAuthzConfigTables_03_version_fn.sql"));

            // Step 4: seed from the CURRENT appsettings values, under the migration actor so the rows are
            // audited. Behaviour is unchanged on deploy EXCEPT admin roles, which seed as PENDING and so lose
            // admin access until approved (disclosed in the design; requirement 4).
            migrationBuilder.Sql(SqlResource.MigrationActor(MigrationName, Gate));
            migrationBuilder.Sql(SqlResource.Read("BudgetAuthzConfigTables_04_seed.sql"));
        }

        private const string MigrationName = "BudgetAuthzConfigTables";
        private const string Gate = "p1_ddd_deviations";

        /// <inheritdoc />
        // Development databases only (per the existing convention). Drop the function and the triggers first,
        // then let EF drop the tables.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP FUNCTION IF EXISTS budget.fn_config_version(); " +
                "DROP TRIGGER IF EXISTS trg_budget_project_settings_require_actor ON budget.budget_project_settings; " +
                "DROP TRIGGER IF EXISTS trg_budget_project_settings_audit ON budget.budget_project_settings; " +
                "DROP TRIGGER IF EXISTS trg_budget_project_settings_forbid_stmt ON budget.budget_project_settings; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_require_actor ON budget.budget_role; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_audit ON budget.budget_role; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_forbid_stmt ON budget.budget_role; " +
                "DROP TRIGGER IF EXISTS trg_budget_right_require_actor ON budget.budget_right; " +
                "DROP TRIGGER IF EXISTS trg_budget_right_audit ON budget.budget_right; " +
                "DROP TRIGGER IF EXISTS trg_budget_right_forbid_stmt ON budget.budget_right; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_right_require_actor ON budget.budget_role_right; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_right_audit ON budget.budget_role_right; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_right_forbid_stmt ON budget.budget_role_right; " +
                "DROP TRIGGER IF EXISTS trg_budget_query_field_require_actor ON budget.budget_query_field; " +
                "DROP TRIGGER IF EXISTS trg_budget_query_field_audit ON budget.budget_query_field; " +
                "DROP TRIGGER IF EXISTS trg_budget_query_field_forbid_stmt ON budget.budget_query_field; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_query_field_require_actor ON budget.budget_role_query_field; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_query_field_audit ON budget.budget_role_query_field; " +
                "DROP TRIGGER IF EXISTS trg_budget_role_query_field_forbid_stmt ON budget.budget_role_query_field; " +
                "DROP TRIGGER IF EXISTS trg_budget_admin_role_assignment_require_actor ON budget.budget_admin_role_assignment; " +
                "DROP TRIGGER IF EXISTS trg_budget_admin_role_assignment_audit ON budget.budget_admin_role_assignment; " +
                "DROP TRIGGER IF EXISTS trg_budget_admin_role_assignment_forbid_stmt ON budget.budget_admin_role_assignment;");

            migrationBuilder.DropTable(
                name: "budget_admin_role_assignment",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_project_settings",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_role_query_field",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_role_right",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_query_field",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_right",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_role",
                schema: "budget");
        }
    }
}
