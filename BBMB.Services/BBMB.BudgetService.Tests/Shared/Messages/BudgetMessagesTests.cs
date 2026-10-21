using BBMB.BudgetService.Shared.Messages;
using FluentAssertions;
using Npgsql;

namespace BBMB.BudgetService.Tests.Shared.Messages;

public sealed class BudgetMessagesTests
{
    private static Dictionary<string, string> SrsSection7()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Shared", "Messages", "srs-section7-messages.txt");
        return File.ReadAllLines(path)
            .Where(l => l.Length > 0 && !l.StartsWith('#'))
            .Select(l => l.Split('\t', 2))
            .ToDictionary(p => p[0], p => p[1]);
    }

    [Fact]
    public void BUD012_every_M01_to_M36_carries_the_SRS_section7_wording()
    {
        var srs = SrsSection7();
        srs.Should().HaveCount(36);

        for (var n = 1; n <= 36; n++)
        {
            var id = $"M-{n:00}";
            BudgetMessages.Text(id).Should().NotBeNullOrWhiteSpace(id);
            BudgetMessages.Text(id).Should().Be(srs[id], id);
        }
    }

    [Fact]
    public void BUD012_format_prefixes_the_id()
    {
        BudgetMessages.Format(BudgetMessages.M19)
            .Should().Be("M-19: This action is not available at the present stage of the proposal.");
    }

    [Fact]
    public void Unknown_id_is_a_coding_defect()
    {
        var act = () => BudgetMessages.Text("M-99");
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static PostgresException Pg(string sqlState, string text, string? table = null, string? constraint = null) =>
        new(text, "ERROR", "ERROR", sqlState, tableName: table, constraintName: constraint);

    [Fact]
    public void BUD012_raised_M06_maps_to_the_catalogue()
    {
        var mapped = BudgetMessages.FromPostgres(Pg(PostgresErrorCodes.RaiseException, "M-06"));
        mapped.Should().NotBeNull();
        mapped!.Value.MessageId.Should().Be("M-06");
        mapped.Value.Text.Should().Be(BudgetMessages.Text("M-06"));
    }

    [Fact]
    public void BUD012_raised_text_that_is_not_an_id_is_unmapped()
    {
        BudgetMessages.FromPostgres(Pg(PostgresErrorCodes.RaiseException, "ACTOR")).Should().BeNull();
        BudgetMessages.FromPostgres(Pg(PostgresErrorCodes.RaiseException, "M-06 extra")).Should().BeNull();
        BudgetMessages.FromPostgres(Pg(PostgresErrorCodes.RaiseException, "M-99")).Should().BeNull();
    }

    [Fact]
    public void BUD012_foreign_key_violation_maps_to_M05_without_table_or_constraint_name()
    {
        var mapped = BudgetMessages.FromPostgres(
            Pg(PostgresErrorCodes.ForeignKeyViolation, "insert violates fk", "demand_line", "fk_demand_line_work"));
        mapped.Should().NotBeNull();
        mapped!.Value.MessageId.Should().Be("M-05");
        mapped.Value.Text.Should().NotContain("demand_line").And.NotContain("fk_demand_line_work");
    }

    [Fact]
    public void Unmapped_sqlstate_returns_null()
    {
        BudgetMessages.FromPostgres(Pg(PostgresErrorCodes.CheckViolation, "check", "work", "ck_work_class")).Should().BeNull();
        BudgetMessages.FromPostgres(Pg(PostgresErrorCodes.UniqueViolation, "dup", "work", "ux_unregistered")).Should().BeNull();
    }
}
