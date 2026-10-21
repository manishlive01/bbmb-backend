using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <summary>
    /// Design §11.4 P6 row, SPLIT (progress JSON scope decision, user time-box 2026-10-15): the P4/P5-INDEPENDENT
    /// half of BudgetP6ConsolidationAndReporting. Creates only the F-26 reporting view over tables that exist now
    /// (v_report_demand, P1/P2/P3) and its conditional bud_report / bud_app grants. The full V-06 v_consolidation
    /// (allocation/provision T-40/T-41/T-42 and re-appropriation T-46) is the deferred sibling P6-01b. Additive,
    /// writes no data, creates no table: no gate, no migration actor (§11.1 rules 8, 9).
    /// </summary>
    public partial class BudgetP6Reporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("BudgetP6Reporting_05_views.sql"));
            migrationBuilder.Sql(SqlResource.Read("BudgetP6Reporting_06_grants.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS budget.v_report_demand;");
        }
    }
}
