using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class heirarchyKeysRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "FK_cr_management_ce_management_ce_code_div_section",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropForeignKey(
                name: "FK_division_ce_management_ce_code_div_section",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_cr_management_cr_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_pool_pool_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_region_region_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_estimate_work_work_code",
                schema: "budget",
                table: "estimate");

            migrationBuilder.DropForeignKey(
                name: "FK_work_division_division_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_work",
                schema: "budget",
                table: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_division",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_ce_code_div_section",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_cr_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cr_management",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropIndex(
                name: "IX_cr_management_ce_code_div_section",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_division_code",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_work_code",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropColumn(
                name: "ce_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "cr_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "ce_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "work_code",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.RenameColumn(
                name: "division_code",
                schema: "budget",
                table: "work",
                newName: "division_id");

            migrationBuilder.RenameColumn(
                name: "div_section",
                schema: "budget",
                table: "work",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_work_division_code",
                schema: "budget",
                table: "work",
                newName: "IX_work_division_id");

            migrationBuilder.RenameColumn(
                name: "div_section",
                schema: "budget",
                table: "division",
                newName: "cr_id");

            migrationBuilder.RenameColumn(
                name: "cr_code",
                schema: "budget",
                table: "division",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "div_section",
                schema: "budget",
                table: "cr_management",
                newName: "ce_id");

            migrationBuilder.RenameColumn(
                name: "ce_code",
                schema: "budget",
                table: "cr_management",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "div_section",
                schema: "budget",
                table: "ce_management",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "division_code",
                schema: "budget",
                table: "budget_table",
                newName: "division_id");

            migrationBuilder.Sql(@"
    ALTER TABLE budget.work 
    ALTER COLUMN work_code TYPE integer 
    USING work_code::integer;
");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "div_section_id",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
    ALTER TABLE budget.estimate 
    ALTER COLUMN work_code TYPE integer 
    USING work_code::integer;
");
            migrationBuilder.AlterColumn<int>(
                name: "division_code",
                schema: "budget",
                table: "division",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "budget",
                table: "division",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "CEManagementId",
                schema: "budget",
                table: "division",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "cr_code",
                schema: "budget",
                table: "cr_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "budget",
                table: "cr_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "budget",
                table: "ce_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "work_id",
                schema: "budget",
                table: "budget_table",
                type: "integer",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_work_work_code",
                schema: "budget",
                table: "work",
                column: "work_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_work",
                schema: "budget",
                table: "work",
                column: "id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_division_division_code",
                schema: "budget",
                table: "division",
                column: "division_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_division",
                schema: "budget",
                table: "division",
                column: "id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_cr_management_cr_code",
                schema: "budget",
                table: "cr_management",
                column: "cr_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cr_management",
                schema: "budget",
                table: "cr_management",
                column: "id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ce_management_ce_code",
                schema: "budget",
                table: "ce_management",
                column: "ce_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management",
                column: "id");

            migrationBuilder.CreateTable(
                name: "cr_section",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cr_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cr_section", x => x.id);
                    table.ForeignKey(
                        name: "FK_cr_section_cr_management_cr_id",
                        column: x => x.cr_id,
                        principalSchema: "budget",
                        principalTable: "cr_management",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "div_section",
                schema: "budget",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    division_id = table.Column<int>(type: "integer", nullable: false),
                    DivisionCode = table.Column<int>(type: "integer", nullable: false),
                    ce_code = table.Column<int>(type: "integer", nullable: false),
                    cr_code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    desc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_div_section", x => x.id);
                    table.ForeignKey(
                        name: "FK_div_section_division_division_id",
                        column: x => x.division_id,
                        principalSchema: "budget",
                        principalTable: "division",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_work_work_code",
                schema: "budget",
                table: "work",
                column: "work_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_division_CEManagementId",
                schema: "budget",
                table: "division",
                column: "CEManagementId");

            migrationBuilder.CreateIndex(
                name: "IX_division_cr_id",
                schema: "budget",
                table: "division",
                column: "cr_id");

            migrationBuilder.CreateIndex(
                name: "IX_division_division_code",
                schema: "budget",
                table: "division",
                column: "division_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cr_management_ce_id",
                schema: "budget",
                table: "cr_management",
                column: "ce_id");

            migrationBuilder.CreateIndex(
                name: "IX_cr_management_cr_code",
                schema: "budget",
                table: "cr_management",
                column: "cr_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ce_management_ce_code",
                schema: "budget",
                table: "ce_management",
                column: "ce_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_division_id_calendar_year_sequence_no",
                schema: "budget",
                table: "budget_table",
                columns: new[] { "division_id", "calendar_year", "sequence_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_table_work_id",
                schema: "budget",
                table: "budget_table",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_cr_section_cr_id",
                schema: "budget",
                table: "cr_section",
                column: "cr_id");

            migrationBuilder.CreateIndex(
                name: "IX_div_section_division_id",
                schema: "budget",
                table: "div_section",
                column: "division_id");

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_division_division_id",
                schema: "budget",
                table: "budget_table",
                column: "division_id",
                principalSchema: "budget",
                principalTable: "division",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_budget_table_work_work_id",
                schema: "budget",
                table: "budget_table",
                column: "work_id",
                principalSchema: "budget",
                principalTable: "work",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cr_management_ce_management_ce_id",
                schema: "budget",
                table: "cr_management",
                column: "ce_id",
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_division_ce_management_CEManagementId",
                schema: "budget",
                table: "division",
                column: "CEManagementId",
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_division_cr_management_cr_id",
                schema: "budget",
                table: "division",
                column: "cr_id",
                principalSchema: "budget",
                principalTable: "cr_management",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_division_pool_pool_code",
                schema: "budget",
                table: "division",
                column: "pool_code",
                principalSchema: "budget",
                principalTable: "pool",
                principalColumn: "pool_code");

            migrationBuilder.AddForeignKey(
                name: "FK_division_region_region_code",
                schema: "budget",
                table: "division",
                column: "region_code",
                principalSchema: "budget",
                principalTable: "region",
                principalColumn: "region_code");

            migrationBuilder.AddForeignKey(
                name: "FK_estimate_work_work_code",
                schema: "budget",
                table: "estimate",
                column: "work_code",
                principalSchema: "budget",
                principalTable: "work",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_work_division_division_id",
                schema: "budget",
                table: "work",
                column: "division_id",
                principalSchema: "budget",
                principalTable: "division",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_division_division_id",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_budget_table_work_work_id",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropForeignKey(
                name: "FK_cr_management_ce_management_ce_id",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropForeignKey(
                name: "FK_division_ce_management_CEManagementId",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_cr_management_cr_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_pool_pool_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_division_region_region_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropForeignKey(
                name: "FK_estimate_work_work_code",
                schema: "budget",
                table: "estimate");

            migrationBuilder.DropForeignKey(
                name: "FK_work_division_division_id",
                schema: "budget",
                table: "work");

            migrationBuilder.DropTable(
                name: "cr_section",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "div_section",
                schema: "budget");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_work_work_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropPrimaryKey(
                name: "PK_work",
                schema: "budget",
                table: "work");

            migrationBuilder.DropIndex(
                name: "IX_work_work_code",
                schema: "budget",
                table: "work");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_division_division_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropPrimaryKey(
                name: "PK_division",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_CEManagementId",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_cr_id",
                schema: "budget",
                table: "division");

            migrationBuilder.DropIndex(
                name: "IX_division_division_code",
                schema: "budget",
                table: "division");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_cr_management_cr_code",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cr_management",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropIndex(
                name: "IX_cr_management_ce_id",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropIndex(
                name: "IX_cr_management_cr_code",
                schema: "budget",
                table: "cr_management");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ce_management_ce_code",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.DropIndex(
                name: "IX_ce_management_ce_code",
                schema: "budget",
                table: "ce_management");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_division_id_calendar_year_sequence_no",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropIndex(
                name: "IX_budget_table_work_id",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.DropColumn(
                name: "div_section_id",
                schema: "budget",
                table: "work");

            migrationBuilder.DropColumn(
                name: "CEManagementId",
                schema: "budget",
                table: "division");

            migrationBuilder.DropColumn(
                name: "work_id",
                schema: "budget",
                table: "budget_table");

            migrationBuilder.RenameColumn(
                name: "division_id",
                schema: "budget",
                table: "work",
                newName: "division_code");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "budget",
                table: "work",
                newName: "div_section");

            migrationBuilder.RenameIndex(
                name: "IX_work_division_id",
                schema: "budget",
                table: "work",
                newName: "IX_work_division_code");

            migrationBuilder.RenameColumn(
                name: "cr_id",
                schema: "budget",
                table: "division",
                newName: "div_section");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "budget",
                table: "division",
                newName: "cr_code");

            migrationBuilder.RenameColumn(
                name: "ce_id",
                schema: "budget",
                table: "cr_management",
                newName: "div_section");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "budget",
                table: "cr_management",
                newName: "ce_code");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "budget",
                table: "ce_management",
                newName: "div_section");

            migrationBuilder.RenameColumn(
                name: "division_id",
                schema: "budget",
                table: "budget_table",
                newName: "division_code");

            migrationBuilder.AlterColumn<string>(
                name: "work_code",
                schema: "budget",
                table: "work",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<int>(
                name: "div_section",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "ce_code",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "cr_code",
                schema: "budget",
                table: "work",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "work_code",
                schema: "budget",
                table: "estimate",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 8);

            migrationBuilder.AlterColumn<int>(
                name: "division_code",
                schema: "budget",
                table: "division",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "cr_code",
                schema: "budget",
                table: "division",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "ce_code",
                schema: "budget",
                table: "division",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "cr_code",
                schema: "budget",
                table: "cr_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "ce_code",
                schema: "budget",
                table: "cr_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "div_section",
                schema: "budget",
                table: "ce_management",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "work_code",
                schema: "budget",
                table: "budget_table",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_work",
                schema: "budget",
                table: "work",
                column: "work_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_division",
                schema: "budget",
                table: "division",
                column: "division_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cr_management",
                schema: "budget",
                table: "cr_management",
                column: "cr_code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ce_management",
                schema: "budget",
                table: "ce_management",
                columns: new[] { "ce_code", "div_section" });

            migrationBuilder.CreateIndex(
                name: "IX_division_ce_code_div_section",
                schema: "budget",
                table: "division",
                columns: new[] { "ce_code", "div_section" });

            migrationBuilder.CreateIndex(
                name: "IX_division_cr_code",
                schema: "budget",
                table: "division",
                column: "cr_code");

            migrationBuilder.CreateIndex(
                name: "IX_cr_management_ce_code_div_section",
                schema: "budget",
                table: "cr_management",
                columns: new[] { "ce_code", "div_section" });

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
                name: "FK_cr_management_ce_management_ce_code_div_section",
                schema: "budget",
                table: "cr_management",
                columns: new[] { "ce_code", "div_section" },
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumns: new[] { "ce_code", "div_section" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_division_ce_management_ce_code_div_section",
                schema: "budget",
                table: "division",
                columns: new[] { "ce_code", "div_section" },
                principalSchema: "budget",
                principalTable: "ce_management",
                principalColumns: new[] { "ce_code", "div_section" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_division_cr_management_cr_code",
                schema: "budget",
                table: "division",
                column: "cr_code",
                principalSchema: "budget",
                principalTable: "cr_management",
                principalColumn: "cr_code",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_estimate_work_work_code",
                schema: "budget",
                table: "estimate",
                column: "work_code",
                principalSchema: "budget",
                principalTable: "work",
                principalColumn: "work_code",
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
    }
}
