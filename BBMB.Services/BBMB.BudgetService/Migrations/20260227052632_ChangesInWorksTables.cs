using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInWorksTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cr_code",
                schema: "budget",
                table: "work",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "original_budget",
                schema: "budget",
                table: "work",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "proposed_budget",
                schema: "budget",
                table: "work",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "remarks",
                schema: "budget",
                table: "work",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "revised_budget",
                schema: "budget",
                table: "work",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cr_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "original_budget",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "proposed_budget",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "remarks",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "revised_budget",
                schema: "budget",
                table: "work");
        }
    }
}
