using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class levelinoffice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "level_id",
                schema: "budget",
                table: "office_management",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_office_management_level_id",
                schema: "budget",
                table: "office_management",
                column: "level_id");

            migrationBuilder.AddForeignKey(
                name: "FK_office_management_levels_level_id",
                schema: "budget",
                table: "office_management",
                column: "level_id",
                principalSchema: "budget",
                principalTable: "levels",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_office_management_levels_level_id",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropIndex(
                name: "IX_office_management_level_id",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropColumn(
                name: "level_id",
                schema: "budget",
                table: "office_management");
        }
    }
}
