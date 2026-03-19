using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class leveltablerefector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Levels",
                schema: "budget",
                table: "Levels");

            migrationBuilder.RenameTable(
                name: "Levels",
                schema: "budget",
                newName: "levels",
                newSchema: "budget");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "budget",
                table: "levels",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "budget",
                table: "levels",
                newName: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_levels",
                schema: "budget",
                table: "levels",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_levels",
                schema: "budget",
                table: "levels");

            migrationBuilder.RenameTable(
                name: "levels",
                schema: "budget",
                newName: "Levels",
                newSchema: "budget");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "budget",
                table: "Levels",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "budget",
                table: "Levels",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Levels",
                schema: "budget",
                table: "Levels",
                column: "Id");
        }
    }
}
