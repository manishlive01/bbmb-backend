using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations.Employee
{
    /// <inheritdoc />
    public partial class AddTitleSourceOfRecruitmentGenderDesignationAppointmentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeCharges_Employees_EmployeeId",
                schema: "employee",
                table: "EmployeeCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_EmploymentDetail_Employees_EmployeeId",
                schema: "employee",
                table: "EmploymentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMember_Employees_EmployeeId",
                schema: "employee",
                table: "FamilyMember");

            migrationBuilder.DropForeignKey(
                name: "FK_PhotoDocument_Employees_EmployeeId",
                schema: "employee",
                table: "PhotoDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhotoDocument",
                schema: "employee",
                table: "PhotoDocument");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FamilyMember",
                schema: "employee",
                table: "FamilyMember");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmploymentDetail",
                schema: "employee",
                table: "EmploymentDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                schema: "employee",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeCharges",
                schema: "employee",
                table: "EmployeeCharges");

            migrationBuilder.RenameTable(
                name: "PhotoDocument",
                schema: "employee",
                newName: "employee_photo_document",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "FamilyMember",
                schema: "employee",
                newName: "family_member",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "EmploymentDetail",
                schema: "employee",
                newName: "employment_detail",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "Employees",
                schema: "employee",
                newName: "employee",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "EmployeeCharges",
                schema: "employee",
                newName: "employee_charge",
                newSchema: "employee");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "employee",
                table: "employee_photo_document",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PhotoUrl",
                schema: "employee",
                table: "employee_photo_document",
                newName: "photo_url");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                schema: "employee",
                table: "employee_photo_document",
                newName: "employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_PhotoDocument_EmployeeId",
                schema: "employee",
                table: "employee_photo_document",
                newName: "IX_employee_photo_document_employee_id");

            migrationBuilder.RenameColumn(
                name: "Relation",
                schema: "employee",
                table: "family_member",
                newName: "relation");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "employee",
                table: "family_member",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Address",
                schema: "employee",
                table: "family_member",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "employee",
                table: "family_member",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PhotoUrl",
                schema: "employee",
                table: "family_member",
                newName: "photo_url");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                schema: "employee",
                table: "family_member",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "IsEmployee",
                schema: "employee",
                table: "family_member",
                newName: "is_employee");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                schema: "employee",
                table: "family_member",
                newName: "employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_FamilyMember_EmployeeId",
                schema: "employee",
                table: "family_member",
                newName: "IX_family_member_employee_id");

            migrationBuilder.RenameColumn(
                name: "Position",
                schema: "employee",
                table: "employment_detail",
                newName: "position");

            migrationBuilder.RenameColumn(
                name: "Designation",
                schema: "employee",
                table: "employment_detail",
                newName: "designation");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "employee",
                table: "employment_detail",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SourceOfRecruitment",
                schema: "employee",
                table: "employment_detail",
                newName: "source_of_recruitment");

            migrationBuilder.RenameColumn(
                name: "RetirementDate",
                schema: "employee",
                table: "employment_detail",
                newName: "retirement_date");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                schema: "employee",
                table: "employment_detail",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "DateOfJoining",
                schema: "employee",
                table: "employment_detail",
                newName: "date_of_joining");

            migrationBuilder.RenameColumn(
                name: "AppointmentType",
                schema: "employee",
                table: "employment_detail",
                newName: "appointment_type");

            migrationBuilder.RenameColumn(
                name: "AppointmentDate",
                schema: "employee",
                table: "employment_detail",
                newName: "appointment_date");

            migrationBuilder.RenameIndex(
                name: "IX_EmploymentDetail_EmployeeId",
                schema: "employee",
                table: "employment_detail",
                newName: "IX_employment_detail_employee_id");

            migrationBuilder.RenameColumn(
                name: "Gender",
                schema: "employee",
                table: "employee",
                newName: "gender");

            migrationBuilder.RenameColumn(
                name: "Email",
                schema: "employee",
                table: "employee",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Address",
                schema: "employee",
                table: "employee",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "employee",
                table: "employee",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "employee",
                table: "employee",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                schema: "employee",
                table: "employee",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "NameHindi",
                schema: "employee",
                table: "employee",
                newName: "name_hindi");

            migrationBuilder.RenameColumn(
                name: "NameEnglish",
                schema: "employee",
                table: "employee",
                newName: "name_english");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                schema: "employee",
                table: "employee",
                newName: "date_of_birth");

            migrationBuilder.RenameColumn(
                name: "ApplicantTitle",
                schema: "employee",
                table: "employee",
                newName: "applicant_title");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "employee",
                table: "employee_charge",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                schema: "employee",
                table: "employee_charge",
                newName: "project_id");

            migrationBuilder.RenameColumn(
                name: "IsChargeExpire",
                schema: "employee",
                table: "employee_charge",
                newName: "is_charge_expire");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "employee",
                table: "employee_charge",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                schema: "employee",
                table: "employee_charge",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "DivCode",
                schema: "employee",
                table: "employee_charge",
                newName: "div_code");

            migrationBuilder.RenameColumn(
                name: "ChargeTime",
                schema: "employee",
                table: "employee_charge",
                newName: "charge_time");

            migrationBuilder.RenameColumn(
                name: "ChargeExpireTime",
                schema: "employee",
                table: "employee_charge",
                newName: "charge_expire_time");

            migrationBuilder.RenameColumn(
                name: "CRCode",
                schema: "employee",
                table: "employee_charge",
                newName: "cr_code");

            migrationBuilder.RenameColumn(
                name: "CECode",
                schema: "employee",
                table: "employee_charge",
                newName: "ce_code");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeCharges_EmployeeId",
                schema: "employee",
                table: "employee_charge",
                newName: "IX_employee_charge_employee_id");

            migrationBuilder.AlterColumn<string>(
                name: "div_code",
                schema: "employee",
                table: "employee_charge",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "cr_code",
                schema: "employee",
                table: "employee_charge",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ce_code",
                schema: "employee",
                table: "employee_charge",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee_photo_document",
                schema: "employee",
                table: "employee_photo_document",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_family_member",
                schema: "employee",
                table: "family_member",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employment_detail",
                schema: "employee",
                table: "employment_detail",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee",
                schema: "employee",
                table: "employee",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_employee_charge",
                schema: "employee",
                table: "employee_charge",
                column: "id");

            migrationBuilder.CreateTable(
                name: "appointment_type_master",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    appointment_type_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_status = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_type_master", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "designation_master",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    designation_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_status = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_designation_master", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gender_master",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gender_name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_status = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gender_master", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "source_of_recruitment_master",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_status = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_of_recruitment_master", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "title_master",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_status = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_title_master", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_employee_charge_employee_employee_id",
                schema: "employee",
                table: "employee_charge",
                column: "employee_id",
                principalSchema: "employee",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employee_photo_document_employee_employee_id",
                schema: "employee",
                table: "employee_photo_document",
                column: "employee_id",
                principalSchema: "employee",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_employment_detail_employee_employee_id",
                schema: "employee",
                table: "employment_detail",
                column: "employee_id",
                principalSchema: "employee",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_family_member_employee_employee_id",
                schema: "employee",
                table: "family_member",
                column: "employee_id",
                principalSchema: "employee",
                principalTable: "employee",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_employee_charge_employee_employee_id",
                schema: "employee",
                table: "employee_charge");

            migrationBuilder.DropForeignKey(
                name: "FK_employee_photo_document_employee_employee_id",
                schema: "employee",
                table: "employee_photo_document");

            migrationBuilder.DropForeignKey(
                name: "FK_employment_detail_employee_employee_id",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropForeignKey(
                name: "FK_family_member_employee_employee_id",
                schema: "employee",
                table: "family_member");

            migrationBuilder.DropTable(
                name: "appointment_type_master",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "designation_master",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "gender_master",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "source_of_recruitment_master",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "title_master",
                schema: "employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_family_member",
                schema: "employee",
                table: "family_member");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employment_detail",
                schema: "employee",
                table: "employment_detail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee_photo_document",
                schema: "employee",
                table: "employee_photo_document");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee_charge",
                schema: "employee",
                table: "employee_charge");

            migrationBuilder.DropPrimaryKey(
                name: "PK_employee",
                schema: "employee",
                table: "employee");

            migrationBuilder.RenameTable(
                name: "family_member",
                schema: "employee",
                newName: "FamilyMember",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "employment_detail",
                schema: "employee",
                newName: "EmploymentDetail",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "employee_photo_document",
                schema: "employee",
                newName: "PhotoDocument",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "employee_charge",
                schema: "employee",
                newName: "EmployeeCharges",
                newSchema: "employee");

            migrationBuilder.RenameTable(
                name: "employee",
                schema: "employee",
                newName: "Employees",
                newSchema: "employee");

            migrationBuilder.RenameColumn(
                name: "relation",
                schema: "employee",
                table: "FamilyMember",
                newName: "Relation");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "employee",
                table: "FamilyMember",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "address",
                schema: "employee",
                table: "FamilyMember",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "employee",
                table: "FamilyMember",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "photo_url",
                schema: "employee",
                table: "FamilyMember",
                newName: "PhotoUrl");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                schema: "employee",
                table: "FamilyMember",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "is_employee",
                schema: "employee",
                table: "FamilyMember",
                newName: "IsEmployee");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                schema: "employee",
                table: "FamilyMember",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_family_member_employee_id",
                schema: "employee",
                table: "FamilyMember",
                newName: "IX_FamilyMember_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "position",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "Position");

            migrationBuilder.RenameColumn(
                name: "designation",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "Designation");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "source_of_recruitment",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "SourceOfRecruitment");

            migrationBuilder.RenameColumn(
                name: "retirement_date",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "RetirementDate");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "date_of_joining",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "DateOfJoining");

            migrationBuilder.RenameColumn(
                name: "appointment_type",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "AppointmentType");

            migrationBuilder.RenameColumn(
                name: "appointment_date",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "AppointmentDate");

            migrationBuilder.RenameIndex(
                name: "IX_employment_detail_employee_id",
                schema: "employee",
                table: "EmploymentDetail",
                newName: "IX_EmploymentDetail_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "employee",
                table: "PhotoDocument",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "photo_url",
                schema: "employee",
                table: "PhotoDocument",
                newName: "PhotoUrl");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                schema: "employee",
                table: "PhotoDocument",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_employee_photo_document_employee_id",
                schema: "employee",
                table: "PhotoDocument",
                newName: "IX_PhotoDocument_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "project_id",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "is_charge_expire",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "IsChargeExpire");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "div_code",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "DivCode");

            migrationBuilder.RenameColumn(
                name: "cr_code",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "CRCode");

            migrationBuilder.RenameColumn(
                name: "charge_time",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "ChargeTime");

            migrationBuilder.RenameColumn(
                name: "charge_expire_time",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "ChargeExpireTime");

            migrationBuilder.RenameColumn(
                name: "ce_code",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "CECode");

            migrationBuilder.RenameIndex(
                name: "IX_employee_charge_employee_id",
                schema: "employee",
                table: "EmployeeCharges",
                newName: "IX_EmployeeCharges_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "gender",
                schema: "employee",
                table: "Employees",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "email",
                schema: "employee",
                table: "Employees",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "address",
                schema: "employee",
                table: "Employees",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "employee",
                table: "Employees",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                schema: "employee",
                table: "Employees",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                schema: "employee",
                table: "Employees",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "name_hindi",
                schema: "employee",
                table: "Employees",
                newName: "NameHindi");

            migrationBuilder.RenameColumn(
                name: "name_english",
                schema: "employee",
                table: "Employees",
                newName: "NameEnglish");

            migrationBuilder.RenameColumn(
                name: "date_of_birth",
                schema: "employee",
                table: "Employees",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "applicant_title",
                schema: "employee",
                table: "Employees",
                newName: "ApplicantTitle");

            migrationBuilder.AlterColumn<string>(
                name: "DivCode",
                schema: "employee",
                table: "EmployeeCharges",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "CRCode",
                schema: "employee",
                table: "EmployeeCharges",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "CECode",
                schema: "employee",
                table: "EmployeeCharges",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FamilyMember",
                schema: "employee",
                table: "FamilyMember",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmploymentDetail",
                schema: "employee",
                table: "EmploymentDetail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhotoDocument",
                schema: "employee",
                table: "PhotoDocument",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeCharges",
                schema: "employee",
                table: "EmployeeCharges",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                schema: "employee",
                table: "Employees",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeCharges_Employees_EmployeeId",
                schema: "employee",
                table: "EmployeeCharges",
                column: "EmployeeId",
                principalSchema: "employee",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmploymentDetail_Employees_EmployeeId",
                schema: "employee",
                table: "EmploymentDetail",
                column: "EmployeeId",
                principalSchema: "employee",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMember_Employees_EmployeeId",
                schema: "employee",
                table: "FamilyMember",
                column: "EmployeeId",
                principalSchema: "employee",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoDocument_Employees_EmployeeId",
                schema: "employee",
                table: "PhotoDocument",
                column: "EmployeeId",
                principalSchema: "employee",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
