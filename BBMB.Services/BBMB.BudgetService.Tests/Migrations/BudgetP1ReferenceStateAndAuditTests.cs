using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>Design §5.3 (T-13, T-15, T-17), §6.1, §11.1 rule 8, §11.4 row 2; DDD §9 L-02, L-03.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP1ReferenceStateAndAuditTests(BudgetDatabaseFixture fixture)
{
    private const string Action = "MIGRATION:BudgetP1ReferenceStateAndAudit";

    [Fact]
    public async Task BUD234_T15_holds_the_sixteen_SRS_states_with_figures_open_per_design_6_1()
    {
        await using var ctx = fixture.NewBudgetContext();
        var states = await ctx.ProposalStates.AsNoTracking().OrderBy(s => s.StateCode).ToListAsync();

        states.Select(s => s.StateCode).Should().Equal(Enumerable.Range(1, 16).Select(n => $"S-{n:00}"));
        states.Where(s => s.FiguresOpen).Select(s => s.StateCode)
            .Should().Equal("S-01", "S-03", "S-04", "S-06", "S-08", "S-10");
        states.Single(s => s.StateCode == "S-16").StateName.Should().Be("Frozen");
        states.Single(s => s.StateCode == "S-09").HolderLevel.Should().Be("FINANCE");
    }

    [Fact]
    public async Task BUD242_T17_holds_the_five_seeded_limits_and_no_unforeseen_head()
    {
        await using var ctx = fixture.NewBudgetContext();
        var settings = await ctx.BudgetSettings.AsNoTracking().ToDictionaryAsync(s => s.SettingKey, s => s.NumericValue);

        settings.Should().BeEquivalentTo(new Dictionary<string, decimal?>
        {
            ["TS_CEILING_PCT"] = 105m,
            ["NO_AA_LIMIT_RUPEES"] = 10000m,
            ["AA_DEMAND_CEILING_PCT"] = 110m,
            ["AMR_INCREASE_PCT"] = 5m,
            ["UPLOAD_MAX_LINES"] = 5000m,
        });
        settings.Should().NotContainKey("UNFORESEEN_ACCOUNT_CODE");
    }

    [Fact]
    public async Task L03_exactly_one_IT_Cell_office_row_exists()
    {
        await using var ctx = fixture.NewBudgetContext();
        var itCell = await ctx.BudgetOffices.AsNoTracking().Where(o => o.OfficeLevel == "IT_CELL").ToListAsync();
        itCell.Should().ContainSingle();
        itCell[0].OfficeName.Should().Be("IT Cell");
        itCell[0].ParentOfficeId.Should().BeNull();
        itCell[0].CreatedBy.Should().Be(fixture.MigrationEmployeeId, "fn_require_actor fills [A] from the migration actor");
        itCell[0].CreatedChargeId.Should().Be(fixture.MigrationChargeId);
    }

    [Fact]
    public async Task BUD036_every_seeded_row_has_a_T51_insert_row_attributed_to_the_executing_officer()
    {
        await using var ctx = fixture.NewBudgetContext();
        var itCellId = await ctx.BudgetOffices.Where(o => o.OfficeLevel == "IT_CELL").Select(o => o.OfficeId).SingleAsync();
        var rows = await ctx.AuditLog.AsNoTracking()
            .Where(a => a.ActionCode == Action && a.Operation == "I")
            .ToListAsync();

        rows.Where(r => r.TableName == "state").Select(r => r.RecordKey).Should()
            .BeEquivalentTo(Enumerable.Range(1, 16).Select(n => $"S-{n:00}"));
        rows.Count(r => r.TableName == "setting").Should().Be(5);
        rows.Where(r => r.TableName == "office").Select(r => r.RecordKey).Should().Equal(itCellId.ToString());

        rows.Should().OnlyContain(r =>
            r.ActorRoleCode == "MIGRATION" &&
            r.ActorName == BudgetDatabaseFixture.MigrationEmployeeName &&
            r.ActorEmployeeId == fixture.MigrationEmployeeId &&
            r.ActorChargeId == fixture.MigrationChargeId &&
            r.ActorOfficeId == itCellId &&
            r.ColumnName == null &&
            r.NewValue != null);
    }

    [Fact]
    public async Task T08_financial_year_dates_are_date_columns_with_the_DDD_checks()
    {
        await using var ctx = fixture.NewBudgetContext();
        var types = await ctx.Database.SqlQueryRaw<string>(
            "SELECT column_name || ':' || data_type AS \"Value\" FROM information_schema.columns " +
            "WHERE table_schema = 'budget' AND table_name = 'fa_financial_years' AND column_name IN ('start_date','end_date')")
            .ToListAsync();
        types.Should().BeEquivalentTo("start_date:date", "end_date:date");

        var bad = () => ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO budget.fa_financial_years (fin_year_closing, start_date, end_date) VALUES (2031, '2030-04-02', '2031-03-31')");
        await bad.Should().ThrowAsync<Npgsql.PostgresException>();
    }
}
