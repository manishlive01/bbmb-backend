using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class addedcharacterlengthonObjectClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "object_acc_desc",
                schema: "budget",
                table: "fa_object_head",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "object_acc_desc",
                schema: "budget",
                table: "fa_object_head",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);
        }
    }
}
