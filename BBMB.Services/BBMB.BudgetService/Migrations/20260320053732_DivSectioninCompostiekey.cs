using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class DivSectioninCompostiekey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cr_management_ce_management_ce_code",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropForeignKey(
                name: "FK_division_ce_management_ce_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_office_management_ce_management_ce_code",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropIndex(
                name: "IX_office_management_ce_code",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropIndex(
                name: "IX_division_ce_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_cr_management_ce_code",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.AddColumn<int>(
                name: "DivSection",
                schema: "budget",
                table: "office_management",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ce_code",
                schema: "budget",
                table: "ce_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management",
                columns: new[] { "ce_code", "div_section" });

            migrationBuilder.CreateIndex(
                name: "IX_office_management_ce_code_DivSection",
                schema: "budget",
                table: "office_management",
                columns: new[] { "ce_code", "DivSection" });

            migrationBuilder.CreateIndex(
                name: "IX_division_ce_code_div_section",
                schema: "budget",
                table: "division",
                columns: new[] { "ce_code", "div_section" });

            migrationBuilder.CreateIndex(
                name: "IX_cr_management_ce_code_div_section",
                schema: "budget",
                table: "cr_management",
                columns: new[] { "ce_code", "div_section" });

            migrationBuilder.AddForeignKey(
                name: "FK_cr_management_ce_management_ce_code_div_section",
                schema: "budget",
                table: "cr_management",
                columns: new[] { "ce_code", "div_section" },
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumns: new[] { "ce_code", "div_section" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_division_ce_management_ce_code_div_section",
                schema: "budget",
                table: "division",
                columns: new[] { "ce_code", "div_section" },
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumns: new[] { "ce_code", "div_section" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_office_management_ce_management_ce_code_DivSection",
                schema: "budget",
                table: "office_management",
                columns: new[] { "ce_code", "DivSection" },
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumns: new[] { "ce_code", "div_section" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cr_management_ce_management_ce_code_div_section",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropForeignKey(
                name: "FK_division_ce_management_ce_code_div_section",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_office_management_ce_management_ce_code_DivSection",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropIndex(
                name: "IX_office_management_ce_code_DivSection",
                schema: "budget",
                table: "office_management");

            migrationBuilder.DropIndex(
                name: "IX_division_ce_code_div_section",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_cr_management_ce_code_div_section",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.DropColumn(
                name: "DivSection",
                schema: "budget",
                table: "office_management");

            migrationBuilder.AlterColumn<int>(
                name: "ce_code",
                schema: "budget",
                table: "ce_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management",
                column: "ce_code");

            migrationBuilder.CreateIndex(
                name: "IX_office_management_ce_code",
                schema: "budget",
                table: "office_management",
                column: "ce_code");

            migrationBuilder.CreateIndex(
                name: "IX_division_ce_code",
                schema: "budget",
                table: "division",
                column: "ce_code");

            migrationBuilder.CreateIndex(
                name: "IX_cr_management_ce_code",
                schema: "budget",
                table: "cr_management",
                column: "ce_code");

            migrationBuilder.AddForeignKey(
                name: "FK_cr_management_ce_management_ce_code",
                schema: "budget",
                table: "cr_management",
                column: "ce_code",
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumn: "ce_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_division_ce_management_ce_code",
                schema: "budget",
                table: "division",
                column: "ce_code",
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumn: "ce_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_office_management_ce_management_ce_code",
                schema: "budget",
                table: "office_management",
                column: "ce_code",
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumn: "ce_code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
