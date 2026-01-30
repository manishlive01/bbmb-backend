using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDetailHeadCodeLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DetailHeadDetailAccCode",
                schema: "budget",
                table: "work",
                type: "character varying(3)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "detail_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_table",
                type: "character varying(3)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(3)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DetailHeadDetailAccCode",
                schema: "budget",
                table: "work",
                type: "character varying(2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "detail_acc_code",
                schema: "budget",
                table: "fa_detail_head",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_table",
                type: "character varying(2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DetailHeadDetailAccCode",
                schema: "budget",
                table: "budget_head_master",
                type: "character varying(2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldNullable: true);
        }
    }
}
