using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations.Employee
{
    /// <inheritdoc />
    public partial class ChnagesInEmployeeChargeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ce_code",
                schema: "employee",
                table: "employee_charge");

            migrationBuilder.DropColumn(
                name: "cr_code",
                schema: "employee",
                table: "employee_charge");

            migrationBuilder.RenameColumn(
                name: "division_code",
                schema: "employee",
                table: "employee_charge",
                newName: "ce_id");

            migrationBuilder.AddColumn<int>(
                name: "cr_id",
                schema: "employee",
                table: "employee_charge",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "division_id",
                schema: "employee",
                table: "employee_charge",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cr_id",
                schema: "employee",
                table: "employee_charge");

            migrationBuilder.DropColumn(
                name: "division_id",
                schema: "employee",
                table: "employee_charge");

            migrationBuilder.RenameColumn(
                name: "ce_id",
                schema: "employee",
                table: "employee_charge",
                newName: "division_code");

            migrationBuilder.AddColumn<int>(
                name: "ce_code",
                schema: "employee",
                table: "employee_charge",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "cr_code",
                schema: "employee",
                table: "employee_charge",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
