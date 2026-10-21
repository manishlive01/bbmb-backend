using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace BBMB.BudgetService.Migrations
{
    /// <summary>
    /// DB-09 write-skew fix (review of P2-01): fn_ts_ceiling_check takes a per-approval advisory lock
    /// before summing. Function body only; no table, data or trigger change, so no gate or migration actor.
    /// </summary>
    public partial class BudgetP2TsCeilingLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("BudgetP2TsCeilingLock_01_function.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlResource.Read("BudgetP2TsCeilingLock_down.sql"));
        }
    }
}
