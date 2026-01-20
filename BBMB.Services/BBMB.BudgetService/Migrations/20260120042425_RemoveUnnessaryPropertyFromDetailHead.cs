using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnnessaryPropertyFromDetailHead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fa_detail_head_fa_major_head_major_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropForeignKey(
                name: "FK_fa_detail_head_fa_minor_head_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropForeignKey(
                name: "FK_fa_detail_head_fa_sub_major_head_sub_major_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropForeignKey(
                name: "FK_fa_detail_head_fa_sub_minor_head_sub_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_detail_head_major_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_detail_head_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_detail_head_sub_major_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_detail_head_sub_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropColumn(
                name: "major_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropColumn(
                name: "minor_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropColumn(
                name: "sub_major_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropColumn(
                name: "sub_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.AddColumn<string>(
                name: "SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_fa_detail_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head",
                column: "SubMinorHeadSubMinorAccCode");

            migrationBuilder.AddForeignKey(
                name: "FK_fa_detail_head_fa_sub_minor_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head",
                column: "SubMinorHeadSubMinorAccCode",
                principalSchema: "budget",
                principalTable: "fa_sub_minor_head",
                principalColumn: "sub_minor_acc_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fa_detail_head_fa_sub_minor_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropIndex(
                name: "IX_fa_detail_head_SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.DropColumn(
                name: "SubMinorHeadSubMinorAccCode",
                schema: "budget",
                table: "fa_detail_head");

            migrationBuilder.AddColumn<string>(
                name: "major_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "minor_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sub_major_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sub_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_fa_detail_head_major_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                column: "major_acc_code");

            migrationBuilder.CreateIndex(
                name: "IX_fa_detail_head_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                column: "minor_acc_code");

            migrationBuilder.CreateIndex(
                name: "IX_fa_detail_head_sub_major_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                column: "sub_major_acc_code");

            migrationBuilder.CreateIndex(
                name: "IX_fa_detail_head_sub_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                column: "sub_minor_acc_code");

            migrationBuilder.AddForeignKey(
                name: "FK_fa_detail_head_fa_major_head_major_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                column: "major_acc_code",
                principalSchema: "budget",
                principalTable: "fa_major_head",
                principalColumn: "major_acc_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fa_detail_head_fa_minor_head_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                column: "minor_acc_code",
                principalSchema: "budget",
                principalTable: "fa_minor_head",
                principalColumn: "minor_acc_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fa_detail_head_fa_sub_major_head_sub_major_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                column: "sub_major_acc_code",
                principalSchema: "budget",
                principalTable: "fa_sub_major_head",
                principalColumn: "sub_major_acc_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fa_detail_head_fa_sub_minor_head_sub_minor_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                column: "sub_minor_acc_code",
                principalSchema: "budget",
                principalTable: "fa_sub_minor_head",
                principalColumn: "sub_minor_acc_code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
