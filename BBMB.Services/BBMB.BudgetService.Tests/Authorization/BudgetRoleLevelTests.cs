using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentAssertions;
namespace BBMB.BudgetService.Tests.Authorization;
/// <summary>
/// The role's office level type is the stored <c>budget_role.office_level</c> code (<see cref="BudgetRoleLevel"/>):
/// no level-name mapping in code. Pure: no database.
/// </summary>
public sealed class BudgetRoleLevelTests
{
    [Theory]
    [InlineData(OfficeLevel.Ce)]
    [InlineData(OfficeLevel.Circle)]
    [InlineData(OfficeLevel.Division)]
    [InlineData(OfficeLevel.Finance)]
    [InlineData(OfficeLevel.Secretariat)]
    [InlineData(OfficeLevel.ItCell)]
    public void A_stored_Budget_office_level_code_is_the_role_type(string code) =>
        BudgetRoleLevel.ToOfficeLevelCode(code).Should().Be(code);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("CR")]
    [InlineData("Div")]
    [InlineData("circle")]
    [InlineData("Sub Division")]
    public void A_null_or_unknown_stored_code_is_unconfigured(string? code) =>
        BudgetRoleLevel.ToOfficeLevelCode(code).Should().BeNull("level names are not interpreted; only the six codes count");

    [Theory]
    [InlineData("BudgetRoleSyncAndLevelFk_02_backfill.sql")]
    [InlineData("BudgetRoleSyncAndLevelFk_down.sql")]
    public void The_historical_migration_SQL_never_reads_levels_status(string file) =>
        SqlResource.Read(file).Should().NotContainEquivalentOf("l.status", "Budget does not read the hierarchy master's status flag");

    [Fact]
    public void The_level_insert_SQL_inserts_exactly_the_Budget_only_levels_if_missing_with_status_false()
    {
        var sql = SqlResource.Read("BudgetRoleSyncAndLevelFk_01_levels.sql");
        sql.Should().Contain("VALUES (1, 'FINANCE'), (2, 'SECRETARIAT'), (3, 'IT_CELL')");
        sql.Should().Contain("WHERE NOT EXISTS (SELECT 1 FROM budget.levels l WHERE upper(btrim(l.name)) = v.name)");
        sql.Should().Contain("SELECT v.name, false");
        sql.Should().NotContainEquivalentOf("UPDATE budget.levels", "existing rows are never updated");
    }
}
