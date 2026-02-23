using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class AddCRAndCEIDInDivisionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ce_id",
                schema: "budget",
                table: "division",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "cr_id",
                schema: "budget",
                table: "division",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_division_ce_id",
                schema: "budget",
                table: "division",
                column: "ce_id");

            migrationBuilder.CreateIndex(
                name: "IX_division_cr_id",
                schema: "budget",
                table: "division",
                column: "cr_id");

            migrationBuilder.AddForeignKey(
                name: "FK_division_ce_management_ce_id",
                schema: "budget",
                table: "division",
                column: "ce_id",
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_division_cr_management_cr_id",
                schema: "budget",
                table: "division",
                column: "cr_id",
                principalSchema: "budget",
                principalTable: "cr_management",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_division_ce_management_ce_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_cr_management_cr_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_ce_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_cr_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "ce_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "cr_id",
                schema: "budget",
                table: "division");
        }
    }
}
