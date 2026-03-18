using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class CreateOfficeManagementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "office_management",
                schema: "budget",
                columns: table => new
                {
                    office_code = table.Column<string>(type: "text", nullable: false),
                    office_desc = table.Column<string>(type: "text", nullable: false),
                    ce_code = table.Column<int>(type: "integer", nullable: false),
                    cr_code = table.Column<int>(type: "integer", nullable: true),
                    division_code = table.Column<int>(type: "integer", nullable: true),
                    modification_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_office_management", x => x.office_code);
                    table.ForeignKey(
                        name: "FK_office_management_ce_management_ce_code",
                        column: x => x.ce_code,
                        principalSchema: "budget",
                        principalTable: "ce_management",
                        principalColumn: "ce_code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_office_management_cr_management_cr_code",
                        column: x => x.cr_code,
                        principalSchema: "budget",
                        principalTable: "cr_management",
                        principalColumn: "cr_code");
                    table.ForeignKey(
                        name: "FK_office_management_division_division_code",
                        column: x => x.division_code,
                        principalSchema: "budget",
                        principalTable: "division",
                        principalColumn: "division_code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_office_management_ce_code",
                schema: "budget",
                table: "office_management",
                column: "ce_code");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "office_management",
                schema: "budget");
        }
    }
}
