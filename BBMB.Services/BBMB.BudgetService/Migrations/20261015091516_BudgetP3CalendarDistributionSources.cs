using System;
using Microsoft.EntityFrameworkCore.Migrations;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    // Design §11.4 P3 row: T-20 calendar, T-39 distribution (+ DB-18 at commit), T-18 expenditure_source_setting;
    // V-05; V-04 v3 (cols 9/10 for the framing office). Additive, no data: no gate, no migration actor.
    public partial class BudgetP3CalendarDistributionSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calendar",
                schema: "budget",
                columns: table => new
                {
                    cycle_id = table.Column<long>(type: "bigint", nullable: false),
                    office_level = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    last_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_charge_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar", x => new { x.cycle_id, x.office_level });
                    table.ForeignKey(
                        name: "FK_calendar_budget_cycle_cycle_id",
                        column: x => x.cycle_id,
                        principalSchema: "budget",
                        principalTable: "budget_cycle",
                        principalColumn: "cycle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "distribution",
                schema: "budget",
                columns: table => new
                {
                    distribution_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    recommendation_id = table.Column<long>(type: "bigint", nullable: false),
                    office_id = table.Column<long>(type: "bigint", nullable: false),
                    rbe_amount = table.Column<decimal>(type: "budget.rupees", nullable: false, defaultValue: 0m),
                    obe_next_amount = table.Column<decimal>(type: "budget.rupees", nullable: false, defaultValue: 0m),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_charge_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_distribution", x => x.distribution_id);
                    table.ForeignKey(
                        name: "FK_distribution_ce_recommendation_recommendation_id",
                        column: x => x.recommendation_id,
                        principalSchema: "budget",
                        principalTable: "ce_recommendation",
                        principalColumn: "recommendation_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_distribution_office_office_id",
                        column: x => x.office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expenditure_source_setting",
                schema: "budget",
                columns: table => new
                {
                    source_setting_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    office_id = table.Column<long>(type: "bigint", nullable: false),
                    fin_year = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    effective_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenditure_source_setting", x => x.source_setting_id);
                    table.ForeignKey(
                        name: "FK_expenditure_source_setting_fa_financial_years_fin_year",
                        column: x => x.fin_year,
                        principalSchema: "budget",
                        principalTable: "fa_financial_years",
                        principalColumn: "fin_year_closing",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expenditure_source_setting_office_office_id",
                        column: x => x.office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_distribution_office_id",
                schema: "budget",
                table: "distribution",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_distribution_recommendation_id_office_id",
                schema: "budget",
                table: "distribution",
                columns: new[] { "recommendation_id", "office_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expenditure_source_setting_fin_year",
                schema: "budget",
                table: "expenditure_source_setting",
                column: "fin_year");

            migrationBuilder.CreateIndex(
                name: "IX_expenditure_source_setting_office_id_fin_year_effective_from",
                schema: "budget",
                table: "expenditure_source_setting",
                columns: new[] { "office_id", "fin_year", "effective_from" },
                unique: true);

            migrationBuilder.Sql(SqlResource.Read("BudgetP3CalendarDistributionSources_02_rules.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP3CalendarDistributionSources_03_functions.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP3CalendarDistributionSources_04_triggers.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP3CalendarDistributionSources_05_views.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP3CalendarDistributionSources_06_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("BudgetP3CalendarDistributionSources_down.sql"));
            migrationBuilder.DropTable(
                name: "calendar",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "distribution",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "expenditure_source_setting",
                schema: "budget");
        }
    }
}
