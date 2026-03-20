using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class DivSectioninHeirarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "div_section",
                schema: "budget",
                table: "cr_management",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "div_section",
                schema: "budget",
                table: "ce_management",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "div_section",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropColumn(
                name: "div_section",
                schema: "budget",
                table: "ce_management");
        }
    }
}
