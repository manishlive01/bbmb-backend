using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB_Budget.Migrations
{
    /// <inheritdoc />
    public partial class AddDistrictMasterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "district_master",
                columns: table => new
                {
                    district_master_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    state_master_id = table.Column<int>(type: "integer", nullable: false),
                    district_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    district_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_district_master", x => x.district_master_id);
                    table.ForeignKey(
                        name: "FK_district_master_state_master_state_master_id",
                        column: x => x.state_master_id,
                        principalTable: "state_master",
                        principalColumn: "state_master_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_district_master_state_master_id",
                table: "district_master",
                column: "state_master_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "district_master");
        }
    }
}
