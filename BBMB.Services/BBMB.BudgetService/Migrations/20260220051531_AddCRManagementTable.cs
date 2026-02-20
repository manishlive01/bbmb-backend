using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class AddCRManagementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cr_management",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ce_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    desc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    user_code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cr_management", x => x.id);
                    table.ForeignKey(
                        name: "FK_cr_management_ce_management_ce_id",
                        column: x => x.ce_id,
                        principalSchema: "budget",
                        principalTable: "ce_management",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ce_management_project_code",
                schema: "budget",
                table: "ce_management",
                column: "project_code");

            migrationBuilder.CreateIndex(
                name: "IX_cr_management_ce_id",
                schema: "budget",
                table: "cr_management",
                column: "ce_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ce_management_projects_project_code",
                schema: "budget",
                table: "ce_management",
                column: "project_code",
                principalSchema: "budget",
                principalTable: "projects",
                principalColumn: "proj_code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ce_management_projects_project_code",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.DropTable(
                name: "cr_management",
                schema: "budget");

            migrationBuilder.DropIndex(
                name: "IX_ce_management_project_code",
                schema: "budget",
                table: "ce_management");
        }
    }
}
