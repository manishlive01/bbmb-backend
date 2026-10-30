using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentAssertions;
namespace BBMB.BudgetService.Tests.Authorization;
/// <summary>
/// The single levels-name → Budget office level mapping (<see cref="BudgetRoleLevel"/>, D3) and the migration SQL
/// that is built from it (D4). Pure: no database.
/// </summary>
public sealed class BudgetRoleLevelTests
{
    [Theory]
    [InlineData("CE", OfficeLevel.Ce)]
    [InlineData("ce", OfficeLevel.Ce)]
    [InlineData("CR", OfficeLevel.Circle)]
    [InlineData(" cr ", OfficeLevel.Circle)]
    [InlineData("Circle", OfficeLevel.Circle)]
    [InlineData("Div", OfficeLevel.Division)]
    [InlineData("DIV", OfficeLevel.Division)]
    [InlineData("Division", OfficeLevel.Division)]
    [InlineData("FINANCE", OfficeLevel.Finance)]
    [InlineData("finance", OfficeLevel.Finance)]
    [InlineData("SECRETARIAT", OfficeLevel.Secretariat)]
    [InlineData("IT_CELL", OfficeLevel.ItCell)]
    public void A_levels_name_in_the_alias_set_maps_to_its_Budget_office_level(string levelName, string expected) =>
        BudgetRoleLevel.ToOfficeLevelCode(levelName).Should().Be(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Sub Division")]
    [InlineData("Chief Engineer")]
    [InlineData("IT CELL")]
    [InlineData("Secretary")]
    [InlineData("Fin")]
    public void A_name_outside_the_alias_set_is_not_a_Budget_office_level(string? levelName) =>
        BudgetRoleLevel.ToOfficeLevelCode(levelName).Should().BeNull();

    [Fact]
    public void Every_Budget_office_level_is_reachable_and_every_alias_targets_one()
    {
        BudgetRoleLevel.Aliases.Values.Distinct().Should().BeEquivalentTo(OfficeLevel.All);
        BudgetRoleLevel.BudgetOnlyLevelNames.Should().OnlyContain(n => BudgetRoleLevel.ToOfficeLevelCode(n) == n);
    }

    [Fact]
    public void The_migration_alias_list_is_generated_from_the_same_map()
    {
        var values = BudgetRoleLevel.SqlAliasValues();

        values.Should().StartWith("VALUES ");
        foreach (var (alias, code) in BudgetRoleLevel.Aliases)
            values.Should().Contain($"('{alias.ToUpperInvariant()}', '{code}')");
        values.Split("), (").Should().HaveCount(BudgetRoleLevel.Aliases.Count);
    }

    [Theory]
    [InlineData("BudgetRoleSyncAndLevelFk_02_backfill.sql")]
    [InlineData("BudgetRoleSyncAndLevelFk_down.sql")]
    public void The_migration_SQL_takes_its_alias_list_from_the_map_and_never_reads_levels_status(string file)
    {
        var raw = SqlResource.Read(file);
        raw.Should().Contain("{{LEVEL_ALIASES}}", "the alias list is substituted, not copied");
        raw.Should().NotContainEquivalentOf("l.status", "Budget does not read the hierarchy master's status flag");
    }

    [Fact]
    public void The_level_insert_SQL_inserts_exactly_the_Budget_only_levels_if_missing_with_status_false()
    {
        var sql = SqlResource.Read("BudgetRoleSyncAndLevelFk_01_levels.sql");

        sql.Should().Contain("VALUES (1, 'FINANCE'), (2, 'SECRETARIAT'), (3, 'IT_CELL')");
        BudgetRoleLevel.BudgetOnlyLevelNames.Should().Equal("FINANCE", "SECRETARIAT", "IT_CELL");
        sql.Should().Contain("WHERE NOT EXISTS (SELECT 1 FROM budget.levels l WHERE upper(btrim(l.name)) = v.name)");
        sql.Should().Contain("SELECT v.name, false");
        sql.Should().NotContainEquivalentOf("UPDATE budget.levels", "existing rows are never updated");
    }
}
