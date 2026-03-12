using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations.Employee
{
    /// <inheritdoc />
    public partial class ChangesInEmployeeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "appointment_type",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "designation",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "source_of_recruitment",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "applicant_title",
                schema: "employee",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "gender",
                schema: "employee",
                table: "employee");

            migrationBuilder.AddColumn<int>(
                name: "GenderId",
                schema: "employee",
                table: "employment_detail",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TitleId",
                schema: "employee",
                table: "employment_detail",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "appointment_type_id",
                schema: "employee",
                table: "employment_detail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "designation_id",
                schema: "employee",
                table: "employment_detail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "source_of_recruitment_id",
                schema: "employee",
                table: "employment_detail",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "gender_id",
                schema: "employee",
                table: "employee",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "title_id",
                schema: "employee",
                table: "employee",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_employment_detail_appointment_type_id",
                schema: "employee",
                table: "employment_detail",
                column: "appointment_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_employment_detail_designation_id",
                schema: "employee",
                table: "employment_detail",
                column: "designation_id");

            migrationBuilder.CreateIndex(
                name: "IX_employment_detail_GenderId",
                schema: "employee",
                table: "employment_detail",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_employment_detail_source_of_recruitment_id",
                schema: "employee",
                table: "employment_detail",
                column: "source_of_recruitment_id");

            migrationBuilder.CreateIndex(
                name: "IX_employment_detail_TitleId",
                schema: "employee",
                table: "employment_detail",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_employee_gender_id",
                schema: "employee",
                table: "employee",
                column: "gender_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_title_id",
                schema: "employee",
                table: "employee",
                column: "title_id");

            migrationBuilder.AddForeignKey(
                name: "FK_employee_gender_master_gender_id",
                schema: "employee",
                table: "employee",
                column: "gender_id",
                principalSchema: "employee",
                principalTable: "gender_master",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_title_master_title_id",
                schema: "employee",
                table: "employee",
                column: "title_id",
                principalSchema: "employee",
                principalTable: "title_master",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employment_detail_appointment_type_master_appointment_type_~",
                schema: "employee",
                table: "employment_detail",
                column: "appointment_type_id",
                principalSchema: "employee",
                principalTable: "appointment_type_master",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employment_detail_designation_master_designation_id",
                schema: "employee",
                table: "employment_detail",
                column: "designation_id",
                principalSchema: "employee",
                principalTable: "designation_master",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employment_detail_gender_master_GenderId",
                schema: "employee",
                table: "employment_detail",
                column: "GenderId",
                principalSchema: "employee",
                principalTable: "gender_master",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_employment_detail_source_of_recruitment_master_source_of_re~",
                schema: "employee",
                table: "employment_detail",
                column: "source_of_recruitment_id",
                principalSchema: "employee",
                principalTable: "source_of_recruitment_master",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employment_detail_title_master_TitleId",
                schema: "employee",
                table: "employment_detail",
                column: "TitleId",
                principalSchema: "employee",
                principalTable: "title_master",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_gender_master_gender_id",
                schema: "employee",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_title_master_title_id",
                schema: "employee",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "FK_employment_detail_appointment_type_master_appointment_type_~",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_employment_detail_designation_master_designation_id",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_employment_detail_gender_master_GenderId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_employment_detail_source_of_recruitment_master_source_of_re~",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_employment_detail_title_master_TitleId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropIndex(
                name: "IX_employment_detail_appointment_type_id",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropIndex(
                name: "IX_employment_detail_designation_id",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropIndex(
                name: "IX_employment_detail_GenderId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropIndex(
                name: "IX_employment_detail_source_of_recruitment_id",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropIndex(
                name: "IX_employment_detail_TitleId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropIndex(
                name: "IX_employee_gender_id",
                schema: "employee",
                table: "employee");

            migrationBuilder.DropIndex(
                name: "IX_employee_title_id",
                schema: "employee",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "GenderId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "TitleId",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "appointment_type_id",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "designation_id",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "source_of_recruitment_id",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropColumn(
                name: "gender_id",
                schema: "employee",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "title_id",
                schema: "employee",
                table: "employee");

            migrationBuilder.AddColumn<string>(
                name: "appointment_type",
                schema: "employee",
                table: "employment_detail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "designation",
                schema: "employee",
                table: "employment_detail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "source_of_recruitment",
                schema: "employee",
                table: "employment_detail",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "applicant_title",
                schema: "employee",
                table: "employee",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "gender",
                schema: "employee",
                table: "employee",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
