using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class addedsectioncode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DivSectionCode",
                schema: "budget",
                table: "div_section",
                newName: "div_section_code");

            migrationBuilder.AddColumn<int>(
                name: "cr_section_code",
                schema: "budget",
                table: "cr_section",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cr_section_code",
                schema: "budget",
                table: "cr_section");

            migrationBuilder.RenameColumn(
                name: "div_section_code",
                schema: "budget",
                table: "div_section",
                newName: "DivSectionCode");
        }
    }
}
