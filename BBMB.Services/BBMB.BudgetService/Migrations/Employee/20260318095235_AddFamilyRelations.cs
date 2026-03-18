using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations.Employee
{
    /// <inheritdoc />
    public partial class AddFamilyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FamilyRelationId",
                schema: "employee",
                table: "employment_detail",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "family_relation_master",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    family_relation_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_status = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_relation_master", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_employment_detail_FamilyRelationId",
                schema: "employee",
                table: "employment_detail",
                column: "FamilyRelationId");

            migrationBuilder.AddForeignKey(
                name: "FK_employment_detail_family_relation_master_FamilyRelationId",
                schema: "employee",
                table: "employment_detail",
                column: "FamilyRelationId",
                principalSchema: "employee",
                principalTable: "family_relation_master",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employment_detail_family_relation_master_FamilyRelationId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropTable(
                name: "family_relation_master",
                schema: "employee");

            migrationBuilder.DropIndex(
                name: "IX_employment_detail_FamilyRelationId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "FamilyRelationId",
                schema: "employee",
                table: "employment_detail");
        }
    }
}
