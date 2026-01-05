using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB_Budget.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetTypeMastersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "budget_type_master",
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
                name: "budget_master",
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
                        principalTable: "budget_type_master",
                        principalColumn: "budget_type_master_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_budget_master_BudgetTypeMasterId",
                table: "budget_master",
                column: "BudgetTypeMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budget_master");

            migrationBuilder.DropTable(
                name: "budget_type_master");
        }
    }
}
