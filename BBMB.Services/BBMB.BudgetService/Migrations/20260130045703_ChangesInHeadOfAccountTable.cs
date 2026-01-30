using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInHeadOfAccountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_head_master_fa_detail_head_DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_head_master_fa_major_head_MajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_head_master_fa_minor_head_MinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_head_master_fa_object_head_ObjectClassificationObjec~",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropPrimaryKey(
                name: "PK_budget_head_master",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_MajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_MinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_ObjectClassificationObjectAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "MajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "MinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropColumn(
                name: "ObjectClassificationObjectAccCode",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.AlterColumn<string>(
                name: "sub_minor_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "sub_major_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(4)",
                maxLength: 4,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "budget",
                table: "budget_head_master",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_budget_head_master",
                schema: "budget",
                table: "budget_head_master",
                columns: new[] { "major_acc_code_fk", "sub_major_acc_code_fk", "minor_acc_code_fk", "sub_minor_acc_code_fk", "detail_acc_code_fk", "object_acc_code_fk" });

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_detail_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                column: "detail_acc_code_fk");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_major_acc_code_fk_sub_major_acc_code_fk_~",
                schema: "budget",
                table: "budget_head_master",
                columns: new[] { "major_acc_code_fk", "sub_major_acc_code_fk", "minor_acc_code_fk", "sub_minor_acc_code_fk", "detail_acc_code_fk", "object_acc_code_fk" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_minor_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                column: "minor_acc_code_fk");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_object_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                column: "object_acc_code_fk");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_head_master_fa_detail_head_detail_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                column: "detail_acc_code_fk",
                principalSchema: "budget",
                principalTable: "fa_detail_head",
                principalColumn: "detail_acc_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_budget_head_master_fa_major_head_major_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                column: "major_acc_code_fk",
                principalSchema: "budget",
                principalTable: "fa_major_head",
                principalColumn: "major_acc_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_budget_head_master_fa_minor_head_minor_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                column: "minor_acc_code_fk",
                principalSchema: "budget",
                principalTable: "fa_minor_head",
                principalColumn: "minor_acc_code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_budget_head_master_fa_object_head_object_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                column: "object_acc_code_fk",
                principalSchema: "budget",
                principalTable: "fa_object_head",
                principalColumn: "object_acc_code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_head_master_fa_detail_head_detail_acc_code_fk",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_head_master_fa_major_head_major_acc_code_fk",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_head_master_fa_minor_head_minor_acc_code_fk",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_head_master_fa_object_head_object_acc_code_fk",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropPrimaryKey(
                name: "PK_budget_head_master",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_detail_acc_code_fk",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_major_acc_code_fk_sub_major_acc_code_fk_~",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_minor_acc_code_fk",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.DropIndex(
                name: "IX_budget_head_master_object_acc_code_fk",
                schema: "budget",
                table: "budget_head_master");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "budget",
                table: "budget_head_master",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "sub_minor_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "sub_major_acc_code_fk",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4)",
                oldMaxLength: 4);

            migrationBuilder.AddColumn<string>(
                name: "DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectClassificationObjectAccCode",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(3)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_budget_head_master",
                schema: "budget",
                table: "budget_head_master",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "DetailHeadDetailAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_MajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "MajorHeadMajorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_MinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "MinorHeadMinorAccCode");

            migrationBuilder.CreateIndex(
                name: "IX_budget_head_master_ObjectClassificationObjectAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "ObjectClassificationObjectAccCode");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_head_master_fa_detail_head_DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "DetailHeadDetailAccCode",
                principalSchema: "budget",
                principalTable: "fa_detail_head",
                principalColumn: "detail_acc_code");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_head_master_fa_major_head_MajorHeadMajorAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "MajorHeadMajorAccCode",
                principalSchema: "budget",
                principalTable: "fa_major_head",
                principalColumn: "major_acc_code");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_head_master_fa_minor_head_MinorHeadMinorAccCode",
                schema: "budget",
                table: "budget_head_master",
                column: "MinorHeadMinorAccCode",
                principalSchema: "budget",
                principalTable: "fa_minor_head",
                principalColumn: "minor_acc_code");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_head_master_fa_object_head_ObjectClassificationObjec~",
                schema: "budget",
                table: "budget_head_master",
                column: "ObjectClassificationObjectAccCode",
                principalSchema: "budget",
                principalTable: "fa_object_head",
                principalColumn: "object_acc_code");
        }
    }
}
