using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BBMB.BudgetService.Migrations
{
    /// <inheritdoc />
    public partial class BudgetP1RupeesAndMoneyColumns : Migration
    {
        /// <inheritdoc />
        // Hand-ordered to design §4.5 (§11.1 rule 2): step 1 domains, step 2 paise guard, step 3 the
        // five ALTER COLUMNs. Left in generated order the ALTERs would fail with "type budget.rupees does
        // not exist" and the guard would never run ahead of them.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: domains budget.rupees and budget.rupees_paise (DDD §1.7, verbatim).
            migrationBuilder.Sql(SqlResource.Read("BudgetP1RupeesAndMoneyColumns_01_domains.sql"));

            // Step 2: guard. Aborts the whole migration if any of the five columns carries paise (OI-09).
            migrationBuilder.Sql(SqlResource.Read("BudgetP1RupeesAndMoneyColumns_02_paise_guard.sql"));

            // Step 3: numeric(12,2) -> budget.rupees (numeric(15,2)) only widens.
            migrationBuilder.AlterColumn<decimal>(
                name: "estimated_cost",
                schema: "budget",
                table: "work",
                type: "budget.rupees",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "estimate_amt",
                schema: "budget",
                table: "estimate",
                type: "budget.rupees",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "revised_amount",
                schema: "budget",
                table: "budget_table",
                type: "budget.rupees",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "proposed_budget",
                schema: "budget",
                table: "budget_table",
                type: "budget.rupees",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "budget_amount",
                schema: "budget",
                table: "budget_table",
                type: "budget.rupees",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)",
                oldPrecision: 12,
                oldScale: 2,
                oldNullable: true);
        }

        /// <inheritdoc />
        // Development databases only (§11.1 rule 6). Narrows back only if every value fits, else raises.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("BudgetP1RupeesAndMoneyColumns_down_narrow_guard.sql"));

            migrationBuilder.AlterColumn<decimal>(
                name: "estimated_cost",
                schema: "budget",
                table: "work",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "budget.rupees",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "estimate_amt",
                schema: "budget",
                table: "estimate",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "budget.rupees",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "revised_amount",
                schema: "budget",
                table: "budget_table",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "budget.rupees",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "proposed_budget",
                schema: "budget",
                table: "budget_table",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "budget.rupees",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "budget_amount",
                schema: "budget",
                table: "budget_table",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "budget.rupees",
                oldNullable: true);

            migrationBuilder.Sql("DROP DOMAIN budget.rupees_paise; DROP DOMAIN budget.rupees;");
        }
    }
}
