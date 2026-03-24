using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class ChnagesInDivSectionManagementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DivisionCode",
                schema: "budget",
                table: "div_section");

            migrationBuilder.DropColumn(
                name: "ce_code",
                schema: "budget",
                table: "div_section");

            migrationBuilder.RenameColumn(
                name: "cr_code",
                schema: "budget",
                table: "div_section",
                newName: "DivSectionCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DivSectionCode",
                schema: "budget",
                table: "div_section",
                newName: "cr_code");

            migrationBuilder.AddColumn<int>(
                name: "DivisionCode",
                schema: "budget",
                table: "div_section",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ce_code",
                schema: "budget",
                table: "div_section",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
