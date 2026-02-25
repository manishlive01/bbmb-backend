using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInCRCandDivisionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cr_management_ce_management_ce_id",
                schema: "budget",
                table: "cr_management");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_cr_management",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropIndex(
                name: "IX_cr_management_ce_id",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.DropColumn(
                name: "ce_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "cr_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropColumn(
                name: "ce_id",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropColumn(
                name: "id",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.AddColumn<string>(
                name: "ce_code",
                schema: "budget",
                table: "division",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "cr_code",
                schema: "budget",
                table: "division",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "cr_code",
                schema: "budget",
                table: "cr_management",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ce_code",
                schema: "budget",
                table: "cr_management",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ce_code",
                schema: "budget",
                table: "ce_management",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cr_management",
                schema: "budget",
                table: "cr_management",
                column: "cr_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management",
                column: "ce_code");

            migrationBuilder.CreateIndex(
                name: "IX_division_ce_code",
                schema: "budget",
                table: "division",
                column: "ce_code");

            migrationBuilder.CreateIndex(
                name: "IX_division_cr_code",
                schema: "budget",
                table: "division",
                column: "cr_code");

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
                name: "FK_division_cr_management_cr_code",
                schema: "budget",
                table: "division",
                column: "cr_code",
                principalSchema: "budget",
                principalTable: "cr_management",
                principalColumn: "cr_code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FK_division_cr_management_cr_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_ce_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_cr_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cr_management",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropIndex(
                name: "IX_cr_management_ce_code",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.DropColumn(
                name: "ce_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "cr_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "cr_code",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropColumn(
                name: "ce_code",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropColumn(
                name: "ce_code",
                schema: "budget",
                table: "ce_management");

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

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "budget",
                table: "cr_management",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "ce_id",
                schema: "budget",
                table: "cr_management",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id",
                schema: "budget",
                table: "ce_management",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_cr_management",
                schema: "budget",
                table: "cr_management",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management",
                column: "id");

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

            migrationBuilder.CreateIndex(
                name: "IX_cr_management_ce_id",
                schema: "budget",
                table: "cr_management",
                column: "ce_id");

            migrationBuilder.AddForeignKey(
                name: "FK_cr_management_ce_management_ce_id",
                schema: "budget",
                table: "cr_management",
                column: "ce_id",
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

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
    }
}
