using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations.Employee
{
    /// <inheritdoc />
    public partial class changesinemployeeAndBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employment_detail_family_relation_master_FamilyRelationId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropIndex(
                name: "IX_employment_detail_FamilyRelationId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "relation",
                schema: "employee",
                table: "family_member");

            migrationBuilder.DropColumn(
                name: "FamilyRelationId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.AddColumn<int>(
                name: "family_relation_id",
                schema: "employee",
                table: "family_member",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "employee_code",
                schema: "employee",
                table: "employee",
                type: "integer",
                maxLength: 6,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_family_member_family_relation_id",
                schema: "employee",
                table: "family_member",
                column: "family_relation_id");

            migrationBuilder.AddForeignKey(
                name: "FK_family_member_family_relation_master_family_relation_id",
                schema: "employee",
                table: "family_member",
                column: "family_relation_id",
                principalSchema: "employee",
                principalTable: "family_relation_master",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_family_member_family_relation_master_family_relation_id",
                schema: "employee",
                table: "family_member");

            migrationBuilder.DropIndex(
                name: "IX_family_member_family_relation_id",
                schema: "employee",
                table: "family_member");

            migrationBuilder.DropColumn(
                name: "family_relation_id",
                schema: "employee",
                table: "family_member");

            migrationBuilder.DropColumn(
                name: "employee_code",
                schema: "employee",
                table: "employee");

            migrationBuilder.AddColumn<string>(
                name: "relation",
                schema: "employee",
                table: "family_member",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FamilyRelationId",
                schema: "employee",
                table: "employment_detail",
                type: "integer",
                nullable: true);

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
    }
}
