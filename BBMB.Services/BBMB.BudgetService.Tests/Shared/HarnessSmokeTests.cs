using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Shared;

[Collection(BudgetDatabaseCollection.Name)]
public sealed class HarnessSmokeTests(BudgetDatabaseFixture fixture)
{
    [Fact]
    public async Task Employee_schema_is_migrated_before_budget()
    {
        await using var emp = fixture.NewEmployeeContext();
        (await emp.EmployeeCharges.AnyAsync(c => c.Id == fixture.MigrationChargeId)).Should().BeTrue();

        await using var budget = fixture.NewBudgetContext();
        var schema = await budget.Database
            .SqlQueryRaw<string>("SELECT to_regclass('employee.employee_charge')::text AS \"Value\"")
            .SingleAsync();
        schema.Should().Be("employee.employee_charge");
    }

    [Fact]
    public async Task Application_session_carries_no_migration_actor()
    {
        await using var budget = fixture.NewBudgetContext();
        var value = await budget.Database
            .SqlQueryRaw<string?>("SELECT current_setting('bbmb_migration.actor_employee_id', true) AS \"Value\"")
            .SingleAsync();
        value.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task Migration_session_carries_the_officer_and_gates_are_TEST()
    {
        await using var budget = fixture.NewMigrationBudgetContext(fixture.PlainConnectionString);
        var actor = await budget.Database
            .SqlQueryRaw<string?>("SELECT current_setting('bbmb_migration.actor_employee_id', true) AS \"Value\"")
            .SingleAsync();
        actor.Should().Be(fixture.MigrationEmployeeId.ToString());

        foreach (var gate in BudgetDatabaseFixture.AllGates)
        {
            var g = await budget.Database
                .SqlQueryRaw<string?>($"SELECT current_setting('bbmb_gate.{gate}', true) AS \"Value\"")
                .SingleAsync();
            g.Should().Be("TEST", gate);
        }
    }

    [Fact]
    public void Endpoint_catalog_lists_POST_api_works()
    {
        EndpointCatalog.Find("POST", "/api/works").Should().NotBeNull();
    }
}
