using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class InitialBudgetSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "budget");

            migrationBuilder.CreateTable(
                name: "budget_type_master",
                schema: "budget",
                columns: table => new
                {
                    budget_type_master_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    budget_type_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    budget_type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_type_master", x => x.budget_type_master_id);
                });

            migrationBuilder.CreateTable(
                name: "state_master",
                schema: "budget",
                columns: table => new
                {
                    state_master_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    state_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    state_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_state_master", x => x.state_master_id);
                });

            migrationBuilder.CreateTable(
                name: "budget_master",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    division_code = table.Column<int>(type: "integer", nullable: false),
                    div_section = table.Column<int>(type: "integer", nullable: false),
                    calendar_year = table.Column<int>(type: "integer", nullable: false),
                    sequence_no = table.Column<int>(type: "integer", nullable: false),
                    major_acc_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    sub_major_acc_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    minor_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    sub_minor_acc_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    detail_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    object_acc_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    work_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    budget_description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    budget_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    revised_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    proposed_budget = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    BudgetTypeMasterId = table.Column<int>(type: "integer", nullable: true),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budget_master", x => x.id);
                    table.ForeignKey(
                        name: "FK_budget_master_budget_type_master_BudgetTypeMasterId",
                        column: x => x.BudgetTypeMasterId,
                        principalSchema: "budget",
                        principalTable: "budget_type_master",
                        principalColumn: "budget_type_master_id");
                });

            migrationBuilder.CreateTable(
                name: "district_master",
                schema: "budget",
                columns: table => new
                {
                    district_master_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    state_master_id = table.Column<int>(type: "integer", nullable: false),
                    district_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    district_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_district_master", x => x.district_master_id);
                    table.ForeignKey(
                        name: "FK_district_master_state_master_state_master_id",
                        column: x => x.state_master_id,
                        principalSchema: "budget",
                        principalTable: "state_master",
                        principalColumn: "state_master_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_master_BudgetTypeMasterId",
                schema: "budget",
                table: "budget_master",
                column: "BudgetTypeMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_district_master_state_master_id",
                schema: "budget",
                table: "district_master",
                column: "state_master_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_master",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "district_master",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "budget_type_master",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "state_master",
                schema: "budget");
        }
    }
}
