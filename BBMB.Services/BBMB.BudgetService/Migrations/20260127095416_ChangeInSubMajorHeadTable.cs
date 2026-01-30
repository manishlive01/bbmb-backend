using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInSubMajorHeadTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_fa_sub_minor_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_fa_detail_head_fa_sub_minor_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropForeignKey(
                name: "FK_work_fa_sub_minor_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fa_sub_minor_head",
                schema: "budget",
                table: "fa_sub_minor_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_sub_minor_head_minor_acc_code",
                schema: "budget",
                table: "fa_sub_minor_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_sub_major_head_major_acc_code",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_detail_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.AddColumn<string>(
                name: "SubMinorHeadMinorAccCode",
                schema: "budget",
                table: "work",
                type: "character varying(3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubMinorHeadMinorAccCode",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubMinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_table",
                type: "character varying(3)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_fa_sub_minor_head",
                schema: "budget",
                table: "fa_sub_minor_head",
                columns: new[] { "minor_acc_code", "sub_minor_acc_code" });

            migrationBuilder.CreateIndex(
                name: "IX_work_SubMinorHeadMinorAccCode_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "work",
                columns: new[] { "SubMinorHeadMinorAccCode", "SubMinorHeadSubMinorAccCode" });

            migrationBuilder.CreateIndex(
                name: "IX_fa_sub_minor_head_minor_acc_code_sub_minor_acc_code",
                schema: "budget",
                table: "fa_sub_minor_head",
                columns: new[] { "minor_acc_code", "sub_minor_acc_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fa_sub_major_head_major_acc_code_sub_major_acc_code",
                schema: "budget",
                table: "fa_sub_major_head",
                columns: new[] { "major_acc_code", "sub_major_acc_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fa_detail_head_SubMinorHeadMinorAccCode_SubMinorHeadSubMino~",
                schema: "budget",
                table: "fa_detail_head",
                columns: new[] { "SubMinorHeadMinorAccCode", "SubMinorHeadSubMinorAccCode" });

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_SubMinorHeadMinorAccCode_SubMinorHeadSubMinorA~",
                schema: "budget",
                table: "budget_table",
                columns: new[] { "SubMinorHeadMinorAccCode", "SubMinorHeadSubMinorAccCode" });

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_fa_sub_minor_head_SubMinorHeadMinorAccCode_Sub~",
                schema: "budget",
                table: "budget_table",
                columns: new[] { "SubMinorHeadMinorAccCode", "SubMinorHeadSubMinorAccCode" },
                principalSchema: "budget",
                principalTable: "fa_sub_minor_head",
                principalColumns: new[] { "minor_acc_code", "sub_minor_acc_code" });

            migrationBuilder.AddForeignKey(
                name: "FK_fa_detail_head_fa_sub_minor_head_SubMinorHeadMinorAccCode_S~",
                schema: "budget",
                table: "fa_detail_head",
                columns: new[] { "SubMinorHeadMinorAccCode", "SubMinorHeadSubMinorAccCode" },
                principalSchema: "budget",
                principalTable: "fa_sub_minor_head",
                principalColumns: new[] { "minor_acc_code", "sub_minor_acc_code" });

            migrationBuilder.AddForeignKey(
                name: "FK_work_fa_sub_minor_head_SubMinorHeadMinorAccCode_SubMinorHea~",
                schema: "budget",
                table: "work",
                columns: new[] { "SubMinorHeadMinorAccCode", "SubMinorHeadSubMinorAccCode" },
                principalSchema: "budget",
                principalTable: "fa_sub_minor_head",
                principalColumns: new[] { "minor_acc_code", "sub_minor_acc_code" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_fa_sub_minor_head_SubMinorHeadMinorAccCode_Sub~",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_fa_detail_head_fa_sub_minor_head_SubMinorHeadMinorAccCode_S~",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropForeignKey(
                name: "FK_work_fa_sub_minor_head_SubMinorHeadMinorAccCode_SubMinorHea~",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_SubMinorHeadMinorAccCode_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fa_sub_minor_head",
                schema: "budget",
                table: "fa_sub_minor_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_sub_minor_head_minor_acc_code_sub_minor_acc_code",
                schema: "budget",
                table: "fa_sub_minor_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_sub_major_head_major_acc_code_sub_major_acc_code",
                schema: "budget",
                table: "fa_sub_major_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_detail_head_SubMinorHeadMinorAccCode_SubMinorHeadSubMino~",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_SubMinorHeadMinorAccCode_SubMinorHeadSubMinorA~",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropColumn(
                name: "SubMinorHeadMinorAccCode",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "SubMinorHeadMinorAccCode",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropColumn(
                name: "SubMinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.AddPrimaryKey(
                name: "PK_fa_sub_minor_head",
                schema: "budget",
                table: "fa_sub_minor_head",
                column: "sub_minor_acc_code");

            migrationBuilder.CreateIndex(
                name: "IX_work_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "work",
                column: "SubMinorHeadSubMinorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_fa_sub_minor_head_minor_acc_code",
                schema: "budget",
                table: "fa_sub_minor_head",
                column: "minor_acc_code");

            migrationBuilder.CreateIndex(
                name: "IX_fa_sub_major_head_major_acc_code",
                schema: "budget",
                table: "fa_sub_major_head",
                column: "major_acc_code");

            migrationBuilder.CreateIndex(
                name: "IX_fa_detail_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head",
                column: "SubMinorHeadSubMinorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "budget_table",
                column: "SubMinorHeadSubMinorAccCode");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_fa_sub_minor_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "budget_table",
                column: "SubMinorHeadSubMinorAccCode",
                principalSchema: "budget",
                principalTable: "fa_sub_minor_head",
                principalColumn: "sub_minor_acc_code");

            migrationBuilder.AddForeignKey(
                name: "FK_fa_detail_head_fa_sub_minor_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head",
                column: "SubMinorHeadSubMinorAccCode",
                principalSchema: "budget",
                principalTable: "fa_sub_minor_head",
                principalColumn: "sub_minor_acc_code");

            migrationBuilder.AddForeignKey(
                name: "FK_work_fa_sub_minor_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "work",
                column: "SubMinorHeadSubMinorAccCode",
                principalSchema: "budget",
                principalTable: "fa_sub_minor_head",
                principalColumn: "sub_minor_acc_code");
        }
    }
}
