using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations.Employee
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "employee");

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

            migrationBuilder.CreateTable(
                name: "employee",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title_id = table.Column<int>(type: "integer", nullable: false),
                    name_english = table.Column<string>(type: "text", nullable: false),
                    name_hindi = table.Column<string>(type: "text", nullable: true),
                    gender_id = table.Column<int>(type: "integer", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_gender_master_gender_id",
                        column: x => x.gender_id,
                        principalSchema: "employee",
                        principalTable: "gender_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employee_title_master_title_id",
                        column: x => x.title_id,
                        principalSchema: "employee",
                        principalTable: "title_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_charge",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_code = table.Column<string>(type: "text", nullable: false),
                    ce_code = table.Column<int>(type: "integer", nullable: false),
                    cr_code = table.Column<int>(type: "integer", nullable: false),
                    division_code = table.Column<int>(type: "integer", nullable: false),
                    charge_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    charge_expire_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_charge_expire = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_charge", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_charge_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "employee",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employee_photo_document",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    photo_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_photo_document", x => x.id);
                    table.ForeignKey(
                        name: "FK_employee_photo_document_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "employee",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employment_detail",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_of_recruitment_id = table.Column<int>(type: "integer", nullable: false),
                    designation_id = table.Column<int>(type: "integer", nullable: false),
                    position = table.Column<string>(type: "text", nullable: false),
                    appointment_type_id = table.Column<int>(type: "integer", nullable: false),
                    appointment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    date_of_joining = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    retirement_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GenderId = table.Column<int>(type: "integer", nullable: true),
                    TitleId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employment_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_employment_detail_appointment_type_master_appointment_type_~",
                        column: x => x.appointment_type_id,
                        principalSchema: "employee",
                        principalTable: "appointment_type_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employment_detail_designation_master_designation_id",
                        column: x => x.designation_id,
                        principalSchema: "employee",
                        principalTable: "designation_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employment_detail_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "employee",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employment_detail_gender_master_GenderId",
                        column: x => x.GenderId,
                        principalSchema: "employee",
                        principalTable: "gender_master",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_employment_detail_source_of_recruitment_master_source_of_re~",
                        column: x => x.source_of_recruitment_id,
                        principalSchema: "employee",
                        principalTable: "source_of_recruitment_master",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employment_detail_title_master_TitleId",
                        column: x => x.TitleId,
                        principalSchema: "employee",
                        principalTable: "title_master",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "family_member",
                schema: "employee",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    relation = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    is_employee = table.Column<bool>(type: "boolean", nullable: false),
                    photo_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_member", x => x.id);
                    table.ForeignKey(
                        name: "FK_family_member_employee_employee_id",
                        column: x => x.employee_id,
                        principalSchema: "employee",
                        principalTable: "employee",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_employee_charge_employee_id",
                schema: "employee",
                table: "employee_charge",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_photo_document_employee_id",
                schema: "employee",
                table: "employee_photo_document",
                column: "employee_id",
                unique: true);

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
                name: "IX_employment_detail_employee_id",
                schema: "employee",
                table: "employment_detail",
                column: "employee_id",
                unique: true);

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
                name: "IX_family_member_employee_id",
                schema: "employee",
                table: "family_member",
                column: "employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_charge",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "employee_photo_document",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "employment_detail",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "family_member",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "appointment_type_master",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "designation_master",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "source_of_recruitment_master",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "employee",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "gender_master",
                schema: "employee");

            migrationBuilder.DropTable(
                name: "title_master",
                schema: "employee");
        }
    }
}
