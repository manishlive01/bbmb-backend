using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInHeadTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_fa_sub_major_head_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_fa_sub_major_head_fa_major_head_major_acc_code",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropForeignKey(
                name: "FK_work_fa_sub_major_head_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fa_sub_major_head",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.AddColumn<string>(
                name: "SubMajorHeadMajorAccCode",
                schema: "budget",
                table: "work",
                type: "character varying(4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MajorHeadMajorAccCode",
                schema: "budget",
                table: "fa_sub_major_head",
                type: "character varying(4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubMajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_table",
                type: "character varying(4)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_fa_sub_major_head",
                schema: "budget",
                table: "fa_sub_major_head",
                columns: new[] { "major_acc_code", "sub_major_acc_code" });

            migrationBuilder.CreateIndex(
                name: "IX_work_SubMajorHeadMajorAccCode_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "work",
                columns: new[] { "SubMajorHeadMajorAccCode", "SubMajorHeadSubMajorAccCode" });

            migrationBuilder.CreateIndex(
                name: "IX_fa_sub_major_head_MajorHeadMajorAccCode",
                schema: "budget",
                table: "fa_sub_major_head",
                column: "MajorHeadMajorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_SubMajorHeadMajorAccCode_SubMajorHeadSubMajorA~",
                schema: "budget",
                table: "budget_table",
                columns: new[] { "SubMajorHeadMajorAccCode", "SubMajorHeadSubMajorAccCode" });

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_fa_sub_major_head_SubMajorHeadMajorAccCode_Sub~",
                schema: "budget",
                table: "budget_table",
                columns: new[] { "SubMajorHeadMajorAccCode", "SubMajorHeadSubMajorAccCode" },
                principalSchema: "budget",
                principalTable: "fa_sub_major_head",
                principalColumns: new[] { "major_acc_code", "sub_major_acc_code" });

            migrationBuilder.AddForeignKey(
                name: "FK_fa_sub_major_head_fa_major_head_MajorHeadMajorAccCode",
                schema: "budget",
                table: "fa_sub_major_head",
                column: "MajorHeadMajorAccCode",
                principalSchema: "budget",
                principalTable: "fa_major_head",
                principalColumn: "major_acc_code");

            migrationBuilder.AddForeignKey(
                name: "FK_fa_sub_major_head_fa_major_head_major_acc_code",
                schema: "budget",
                table: "fa_sub_major_head",
                column: "major_acc_code",
                principalSchema: "budget",
                principalTable: "fa_major_head",
                principalColumn: "major_acc_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_fa_sub_major_head_SubMajorHeadMajorAccCode_SubMajorHea~",
                schema: "budget",
                table: "work",
                columns: new[] { "SubMajorHeadMajorAccCode", "SubMajorHeadSubMajorAccCode" },
                principalSchema: "budget",
                principalTable: "fa_sub_major_head",
                principalColumns: new[] { "major_acc_code", "sub_major_acc_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_fa_sub_major_head_SubMajorHeadMajorAccCode_Sub~",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_fa_sub_major_head_fa_major_head_MajorHeadMajorAccCode",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropForeignKey(
                name: "FK_fa_sub_major_head_fa_major_head_major_acc_code",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropForeignKey(
                name: "FK_work_fa_sub_major_head_SubMajorHeadMajorAccCode_SubMajorHea~",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_SubMajorHeadMajorAccCode_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fa_sub_major_head",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_sub_major_head_MajorHeadMajorAccCode",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_SubMajorHeadMajorAccCode_SubMajorHeadSubMajorA~",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropColumn(
                name: "SubMajorHeadMajorAccCode",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "MajorHeadMajorAccCode",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropColumn(
                name: "SubMajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.AddPrimaryKey(
                name: "PK_fa_sub_major_head",
                schema: "budget",
                table: "fa_sub_major_head",
                column: "sub_major_acc_code");

            migrationBuilder.CreateIndex(
                name: "IX_work_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "work",
                column: "SubMajorHeadSubMajorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "budget_table",
                column: "SubMajorHeadSubMajorAccCode");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_fa_sub_major_head_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "budget_table",
                column: "SubMajorHeadSubMajorAccCode",
                principalSchema: "budget",
                principalTable: "fa_sub_major_head",
                principalColumn: "sub_major_acc_code");

            migrationBuilder.AddForeignKey(
                name: "FK_fa_sub_major_head_fa_major_head_major_acc_code",
                schema: "budget",
                table: "fa_sub_major_head",
                column: "major_acc_code",
                principalSchema: "budget",
                principalTable: "fa_major_head",
                principalColumn: "major_acc_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_work_fa_sub_major_head_SubMajorHeadSubMajorAccCode",
                schema: "budget",
                table: "work",
                column: "SubMajorHeadSubMajorAccCode",
                principalSchema: "budget",
                principalTable: "fa_sub_major_head",
                principalColumn: "sub_major_acc_code");
        }
    }
}
