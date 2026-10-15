using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <summary>
    /// Design §11.4 P2 row 3: T-34 upload_batch (DDD T-34, F-08, BUD-101) and the T-30 upload_batch_id reference
    /// (DDD T-30, BUD-101). T-34 is append-only (DB-27). Additive and writes no data, so it needs neither a gate
    /// nor the migration actor (§11.1 rules 8, 9). Up() is hand-ordered: the EF table, indexes and in-schema FKs,
    /// then the SQL — the lines_committed CHECK and the cross-schema actor FKs (rules), the triggers, the grants
    /// (§11.1 rule 2).
    /// </summary>
    public partial class BudgetP2UploadBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "upload_batch",
                schema: "budget",
                columns: table => new
                {
                    upload_batch_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    proposal_id = table.Column<long>(type: "bigint", nullable: false),
                    file_attachment_id = table.Column<long>(type: "bigint", nullable: false),
                    lines_committed = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_upload_batch", x => x.upload_batch_id);
                    table.ForeignKey(
                        name: "FK_upload_batch_attachment_file_attachment_id",
                        column: x => x.file_attachment_id,
                        principalSchema: "budget",
                        principalTable: "attachment",
                        principalColumn: "attachment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_upload_batch_proposal_proposal_id",
                        column: x => x.proposal_id,
                        principalSchema: "budget",
                        principalTable: "proposal",
                        principalColumn: "proposal_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_demand_line_upload_batch_id",
                schema: "budget",
                table: "demand_line",
                column: "upload_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_upload_batch_file_attachment_id",
                schema: "budget",
                table: "upload_batch",
                column: "file_attachment_id");

            migrationBuilder.CreateIndex(
                name: "IX_upload_batch_proposal_id",
                schema: "budget",
                table: "upload_batch",
                column: "proposal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_demand_line_upload_batch_upload_batch_id",
                schema: "budget",
                table: "demand_line",
                column: "upload_batch_id",
                principalSchema: "budget",
                principalTable: "upload_batch",
                principalColumn: "upload_batch_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(SqlResource.Read("BudgetP2UploadBatch_02_rules.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2UploadBatch_04_triggers.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP2UploadBatch_06_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_demand_line_upload_batch_upload_batch_id",
                schema: "budget",
                table: "demand_line");

            migrationBuilder.DropTable(
                name: "upload_batch",
                schema: "budget");

            migrationBuilder.DropIndex(
                name: "IX_demand_line_upload_batch_id",
                schema: "budget",
                table: "demand_line");
        }
    }
}
