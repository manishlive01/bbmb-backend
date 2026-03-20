using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class officekeysresolved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_office_management_ce_management_ce_code_DivSection",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropForeignKey(
                name: "FK_office_management_cr_management_cr_code",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropForeignKey(
                name: "FK_office_management_division_division_code",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropIndex(
                name: "IX_office_management_ce_code_DivSection",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropIndex(
                name: "IX_office_management_cr_code",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropIndex(
                name: "IX_office_management_division_code",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropColumn(
                name: "DivSection",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropColumn(
                name: "ce_code",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropColumn(
                name: "cr_code",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropColumn(
                name: "division_code",
                schema: "budget",
                table: "office_management");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DivSection",
                schema: "budget",
                table: "office_management",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ce_code",
                schema: "budget",
                table: "office_management",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "cr_code",
                schema: "budget",
                table: "office_management",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "division_code",
                schema: "budget",
                table: "office_management",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_office_management_ce_code_DivSection",
                schema: "budget",
                table: "office_management",
                columns: new[] { "ce_code", "DivSection" });

            migrationBuilder.CreateIndex(
                name: "IX_office_management_cr_code",
                schema: "budget",
                table: "office_management",
                column: "cr_code");

            migrationBuilder.CreateIndex(
                name: "IX_office_management_division_code",
                schema: "budget",
                table: "office_management",
                column: "division_code");

            migrationBuilder.AddForeignKey(
                name: "FK_office_management_ce_management_ce_code_DivSection",
                schema: "budget",
                table: "office_management",
                columns: new[] { "ce_code", "DivSection" },
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumns: new[] { "ce_code", "div_section" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_office_management_cr_management_cr_code",
                schema: "budget",
                table: "office_management",
                column: "cr_code",
                principalSchema: "budget",
                principalTable: "cr_management",
                principalColumn: "cr_code");

            migrationBuilder.AddForeignKey(
                name: "FK_office_management_division_division_code",
                schema: "budget",
                table: "office_management",
                column: "division_code",
                principalSchema: "budget",
                principalTable: "division",
                principalColumn: "division_code");
        }
    }
}
