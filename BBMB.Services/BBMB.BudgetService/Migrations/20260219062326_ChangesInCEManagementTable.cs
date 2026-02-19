using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInCEManagementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Convert varchar id to integer using explicit cast
            migrationBuilder.Sql(@"
                ALTER TABLE budget.ce_management
                ALTER COLUMN id TYPE integer
                USING id::integer;
            ");

            // Step 2: Set identity (auto increment)
            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "budget",
                table: "ce_management",
                type: "integer",
                nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove identity
            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "budget",
                table: "ce_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int))
                .OldAnnotation("Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // Convert back to varchar(3)
            migrationBuilder.Sql(@"
                ALTER TABLE budget.ce_management
                ALTER COLUMN id TYPE varchar(3)
                USING id::varchar;
            ");
        }
    }
}
