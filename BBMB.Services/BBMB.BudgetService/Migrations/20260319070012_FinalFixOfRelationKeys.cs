using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class FinalFixOfRelationKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_division_DivisionCode1",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_work_WorkCode1",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_division_pool_PoolCode1",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_region_RegionCode1",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_work_designation_DesignationCode1",
                schema: "budget",
                table: "work");

            migrationBuilder.DropForeignKey(
                name: "FK_work_division_DivisionCode1",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_DesignationCode1",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_DivisionCode1",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_division_PoolCode1",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_RegionCode1",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_DivisionCode1",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_WorkCode1",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropColumn(
                name: "DesignationCode1",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "DivisionCode1",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "PoolCode1",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "RegionCode1",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "DivisionCode1",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropColumn(
                name: "WorkCode1",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.CreateIndex(
                name: "IX_work_designation_code",
                schema: "budget",
                table: "work",
                column: "designation_code");

            migrationBuilder.CreateIndex(
                name: "IX_work_division_code",
                schema: "budget",
                table: "work",
                column: "division_code");

            migrationBuilder.CreateIndex(
                name: "IX_division_pool_code",
                schema: "budget",
                table: "division",
                column: "pool_code");

            migrationBuilder.CreateIndex(
                name: "IX_division_region_code",
                schema: "budget",
                table: "division",
                column: "region_code");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_division_code",
                schema: "budget",
                table: "budget_table",
                column: "division_code");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_work_code",
                schema: "budget",
                table: "budget_table",
                column: "work_code");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_division_division_code",
                schema: "budget",
                table: "budget_table",
                column: "division_code",
                principalSchema: "budget",
                principalTable: "division",
                principalColumn: "division_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_work_work_code",
                schema: "budget",
                table: "budget_table",
                column: "work_code",
                principalSchema: "budget",
                principalTable: "work",
                principalColumn: "work_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_division_pool_pool_code",
                schema: "budget",
                table: "division",
                column: "pool_code",
                principalSchema: "budget",
                principalTable: "pool",
                principalColumn: "pool_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_division_region_region_code",
                schema: "budget",
                table: "division",
                column: "region_code",
                principalSchema: "budget",
                principalTable: "region",
                principalColumn: "region_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_designation_designation_code",
                schema: "budget",
                table: "work",
                column: "designation_code",
                principalSchema: "budget",
                principalTable: "designation",
                principalColumn: "designation_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_division_division_code",
                schema: "budget",
                table: "work",
                column: "division_code",
                principalSchema: "budget",
                principalTable: "division",
                principalColumn: "division_code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_division_division_code",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_work_work_code",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_division_pool_pool_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_region_region_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_work_designation_designation_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropForeignKey(
                name: "FK_work_division_division_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_designation_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_division_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_division_pool_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_region_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_division_code",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_work_code",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.AddColumn<string>(
                name: "DesignationCode1",
                schema: "budget",
                table: "work",
                type: "character varying(4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DivisionCode1",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PoolCode1",
                schema: "budget",
                table: "division",
                type: "character varying(2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionCode1",
                schema: "budget",
                table: "division",
                type: "character varying(2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DivisionCode1",
                schema: "budget",
                table: "budget_table",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkCode1",
                schema: "budget",
                table: "budget_table",
                type: "character varying(8)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_DesignationCode1",
                schema: "budget",
                table: "work",
                column: "DesignationCode1");

            migrationBuilder.CreateIndex(
                name: "IX_work_DivisionCode1",
                schema: "budget",
                table: "work",
                column: "DivisionCode1");

            migrationBuilder.CreateIndex(
                name: "IX_division_PoolCode1",
                schema: "budget",
                table: "division",
                column: "PoolCode1");

            migrationBuilder.CreateIndex(
                name: "IX_division_RegionCode1",
                schema: "budget",
                table: "division",
                column: "RegionCode1");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_DivisionCode1",
                schema: "budget",
                table: "budget_table",
                column: "DivisionCode1");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_WorkCode1",
                schema: "budget",
                table: "budget_table",
                column: "WorkCode1");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_division_DivisionCode1",
                schema: "budget",
                table: "budget_table",
                column: "DivisionCode1",
                principalSchema: "budget",
                principalTable: "division",
                principalColumn: "division_code");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_work_WorkCode1",
                schema: "budget",
                table: "budget_table",
                column: "WorkCode1",
                principalSchema: "budget",
                principalTable: "work",
                principalColumn: "work_code");

            migrationBuilder.AddForeignKey(
                name: "FK_division_pool_PoolCode1",
                schema: "budget",
                table: "division",
                column: "PoolCode1",
                principalSchema: "budget",
                principalTable: "pool",
                principalColumn: "pool_code");

            migrationBuilder.AddForeignKey(
                name: "FK_division_region_RegionCode1",
                schema: "budget",
                table: "division",
                column: "RegionCode1",
                principalSchema: "budget",
                principalTable: "region",
                principalColumn: "region_code");

            migrationBuilder.AddForeignKey(
                name: "FK_work_designation_DesignationCode1",
                schema: "budget",
                table: "work",
                column: "DesignationCode1",
                principalSchema: "budget",
                principalTable: "designation",
                principalColumn: "designation_code");

            migrationBuilder.AddForeignKey(
                name: "FK_work_division_DivisionCode1",
                schema: "budget",
                table: "work",
                column: "DivisionCode1",
                principalSchema: "budget",
                principalTable: "division",
                principalColumn: "division_code");
        }
    }
}
