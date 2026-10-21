using System;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetP1FiguresAndRecommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-ordered (design §11.1 rule 2): gate guard first, then the generated DDL, then the hand SQL.
            migrationBuilder.Sql(SqlResource.Read("BudgetP1FiguresAndRecommendations_00_gate.sql"));

            migrationBuilder.CreateTable(
                name: "ce_recommendation",
                schema: "budget",
                columns: table => new
                {
                    recommendation_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cycle_id = table.Column<long>(type: "bigint", nullable: false),
                    category = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ce_office_id = table.Column<long>(type: "bigint", nullable: false),
                    account_code_id = table.Column<long>(type: "bigint", nullable: false),
                    rbe_recommended = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    obe_next_recommended = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    concurrence_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    redistribution_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    distribution_confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    distribution_confirmed_by = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ce_recommendation", x => x.recommendation_id);
                    table.ForeignKey(
                        name: "FK_ce_recommendation_budget_cycle_cycle_id",
                        column: x => x.cycle_id,
                        principalSchema: "budget",
                        principalTable: "budget_cycle",
                        principalColumn: "cycle_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ce_recommendation_office_ce_office_id",
                        column: x => x.ce_office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expenditure_figure",
                schema: "budget",
                columns: table => new
                {
                    expenditure_figure_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    subject_type = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    work_id = table.Column<int>(type: "integer", nullable: true),
                    office_id = table.Column<long>(type: "bigint", nullable: true),
                    account_code_id = table.Column<long>(type: "bigint", nullable: true),
                    nature = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    fin_year = table.Column<int>(type: "integer", nullable: false),
                    period = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    up_to_month = table.Column<DateOnly>(type: "date", nullable: true),
                    amount = table.Column<decimal>(type: "budget.rupees_paise", nullable: false),
                    source = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    as_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenditure_figure", x => x.expenditure_figure_id);
                    table.ForeignKey(
                        name: "FK_expenditure_figure_fa_financial_years_fin_year",
                        column: x => x.fin_year,
                        principalSchema: "budget",
                        principalTable: "fa_financial_years",
                        principalColumn: "fin_year_closing",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expenditure_figure_office_office_id",
                        column: x => x.office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expenditure_figure_work_work_id",
                        column: x => x.work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "opening_figure",
                schema: "budget",
                columns: table => new
                {
                    opening_figure_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    cycle_id = table.Column<long>(type: "bigint", nullable: false),
                    subject_type = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    work_id = table.Column<int>(type: "integer", nullable: true),
                    office_id = table.Column<long>(type: "bigint", nullable: true),
                    account_code_id = table.Column<long>(type: "bigint", nullable: true),
                    figure_kind = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    amount = table.Column<decimal>(type: "budget.rupees", nullable: false),
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
                    table.PrimaryKey("PK_opening_figure", x => x.opening_figure_id);
                    table.ForeignKey(
                        name: "FK_opening_figure_budget_cycle_cycle_id",
                        column: x => x.cycle_id,
                        principalSchema: "budget",
                        principalTable: "budget_cycle",
                        principalColumn: "cycle_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_opening_figure_office_office_id",
                        column: x => x.office_id,
                        principalSchema: "budget",
                        principalTable: "office",
                        principalColumn: "office_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_opening_figure_work_work_id",
                        column: x => x.work_id,
                        principalSchema: "budget",
                        principalTable: "work",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recommendation_version",
                schema: "budget",
                columns: table => new
                {
                    recommendation_id = table.Column<long>(type: "bigint", nullable: false),
                    version_no = table.Column<short>(type: "smallint", nullable: false),
                    rbe_recommended = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    obe_next_recommended = table.Column<decimal>(type: "budget.rupees", nullable: true),
                    concurrence_ref = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_charge_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendation_version", x => new { x.recommendation_id, x.version_no });
                    table.ForeignKey(
                        name: "FK_recommendation_version_ce_recommendation_recommendation_id",
                        column: x => x.recommendation_id,
                        principalSchema: "budget",
                        principalTable: "ce_recommendation",
                        principalColumn: "recommendation_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ce_recommendation_ce_office_id",
                schema: "budget",
                table: "ce_recommendation",
                column: "ce_office_id");

            migrationBuilder.CreateIndex(
                name: "IX_ce_recommendation_cycle_id_category_ce_office_id_account_co~",
                schema: "budget",
                table: "ce_recommendation",
                columns: new[] { "cycle_id", "category", "ce_office_id", "account_code_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expenditure_figure_fin_year",
                schema: "budget",
                table: "expenditure_figure",
                column: "fin_year");

            migrationBuilder.CreateIndex(
                name: "IX_expenditure_figure_office_id",
                schema: "budget",
                table: "expenditure_figure",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_expenditure_figure_subject_type_work_id_office_id_account_c~",
                schema: "budget",
                table: "expenditure_figure",
                columns: new[] { "subject_type", "work_id", "office_id", "account_code_id", "nature", "fin_year", "period" });

            migrationBuilder.CreateIndex(
                name: "IX_expenditure_figure_work_id",
                schema: "budget",
                table: "expenditure_figure",
                column: "work_id");

            migrationBuilder.CreateIndex(
                name: "IX_opening_figure_cycle_id_subject_type_work_id_office_id_acco~",
                schema: "budget",
                table: "opening_figure",
                columns: new[] { "cycle_id", "subject_type", "work_id", "office_id", "account_code_id", "figure_kind" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_opening_figure_office_id",
                schema: "budget",
                table: "opening_figure",
                column: "office_id");

            migrationBuilder.CreateIndex(
                name: "IX_opening_figure_work_id",
                schema: "budget",
                table: "opening_figure",
                column: "work_id");
        
            migrationBuilder.Sql(SqlResource.Read("BudgetP1FiguresAndRecommendations_02_rules.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1FiguresAndRecommendations_03_functions.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1FiguresAndRecommendations_04_triggers.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1FiguresAndRecommendations_05_views.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP1FiguresAndRecommendations_06_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW budget.v_demand_line_display; DROP VIEW budget.v_demand_by_ce;");

            migrationBuilder.DropTable(
                name: "expenditure_figure",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "opening_figure",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "recommendation_version",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "ce_recommendation",
                schema: "budget");

            migrationBuilder.Sql("DROP FUNCTION budget.fn_opening_figure_rules(); DROP FUNCTION budget.fn_recommendation_rules(); DROP FUNCTION budget.fn_capture_recommendation_version();");
        }
    }
}
