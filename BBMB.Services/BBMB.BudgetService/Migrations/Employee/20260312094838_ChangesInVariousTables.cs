using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations.Employee
{
    /// <inheritdoc />
    public partial class ChangesInVariousTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "div_code",
                schema: "employee",
                table: "employee_charge");

            migrationBuilder.RenameColumn(
                name: "project_id",
                schema: "employee",
                table: "employee_charge",
                newName: "division_code");

            migrationBuilder.AddColumn<string>(
                name: "project_code",
                schema: "employee",
                table: "employee_charge",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "project_code",
                schema: "employee",
                table: "employee_charge");

            migrationBuilder.RenameColumn(
                name: "division_code",
                schema: "employee",
                table: "employee_charge",
                newName: "project_id");

            migrationBuilder.AddColumn<string>(
                name: "div_code",
                schema: "employee",
                table: "employee_charge",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
