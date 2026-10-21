using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>Design §4.5, §11.1 rules 2 and 4; DDD DV-02.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP1RupeesAndMoneyColumnsTests(BudgetDatabaseFixture fixture)
{
    private const string PreviousMigration = "20261014121159_BudgetP0ReconcileBudgetEntryDivSectionColumn";
    private const string ThisMigration = "20261014133303_BudgetP1RupeesAndMoneyColumns";

    private static readonly (string Table, string Column)[] MoneyColumns =
    [
        ("budget_table", "budget_amount"),
        ("budget_table", "revised_amount"),
        ("budget_table", "proposed_budget"),
        ("estimate", "estimate_amt"),
        ("work", "estimated_cost"),
    ];

    private static async Task<string?> ScalarAsync(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? null : v.ToString();
    }

    [Fact]
    public async Task DV02_MoneyColumnsUseRupeesDomain()
    {
        foreach (var (table, column) in MoneyColumns)
        {
            var domain = await ScalarAsync(fixture.PlainConnectionString,
                $"SELECT domain_schema || '.' || domain_name FROM information_schema.columns " +
                $"WHERE table_schema = 'budget' AND table_name = '{table}' AND column_name = '{column}'");
            domain.Should().Be("budget.rupees", $"{table}.{column}");
        }

        // The domain CHECK refuses paise, whatever writes the row (DV-02).
        var act = () => ScalarAsync(fixture.PlainConnectionString, "SELECT 100.50::budget.rupees");
        (await act.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);

        (await ScalarAsync(fixture.PlainConnectionString, "SELECT (-500)::budget.rupees")).Should().Be("-500.00");
        var tooLarge = () => ScalarAsync(fixture.PlainConnectionString, "SELECT 10000000000000::budget.rupees");
        await tooLarge.Should().ThrowAsync<PostgresException>();
    }

    [Fact]
    public async Task DV02_PaiseGuardAbortsBeforeTypeChange()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("p1_paise_guard");
        await fixture.MigrateToAsync(cs, PreviousMigration);

        // Plant a legacy value carrying paise. The estimate FK chain (work → division → circle → ...)
        // is irrelevant to the guard, so FK triggers are bypassed for this one seeding statement only
        // (session_replication_role, test data only; migrations never disable triggers, §11.1 rule 8).
        await using (var conn = new NpgsqlConnection(cs))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SET session_replication_role = replica; " +
                "INSERT INTO budget.estimate (work_code, estimate_code, estimate_amt, fin_year_end, division_code) " +
                "VALUES (1, 'E1', 100.50, 2026, 1); " +
                "SET session_replication_role = origin;", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        var act = () => fixture.MigrateToAsync(cs, ThisMigration);
        var ex = (await act.Should().ThrowAsync<PostgresException>()).Which;
        ex.MessageText.Should().Contain("estimate.estimate_amt: 1 row(s)");
        ex.MessageText.Should().NotContain("does not exist");

        (await ScalarAsync(cs,
                "SELECT format_type(atttypid, atttypmod) FROM pg_attribute " +
                "WHERE attrelid = 'budget.estimate'::regclass AND attname = 'estimate_amt'"))
            .Should().Be("numeric(12,2)");
        (await ScalarAsync(cs, "SELECT to_regtype('budget.rupees')::text")).Should().BeNull("the migration rolled back");
    }
}
