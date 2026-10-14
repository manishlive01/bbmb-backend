using BBMB.BudgetService.Shared.Services.BudgetActors;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>
/// Design §14 TestActor: inserts an employee with an active charge and returns a configured
/// <see cref="FakeBudgetActorSource"/> acting from <paramref name="officeId"/> (the IT Cell when omitted).
/// </summary>
public static class TestActor
{
    public static async Task<FakeBudgetActorSource> CreateAsync(
        BudgetDatabaseFixture fixture,
        string role = "IT_CELL",
        long? officeId = null,
        string officeLevel = "IT_CELL",
        string name = "Test Officer",
        string? connectionString = null,
        int? divisionId = null,
        int? crId = null,
        int? ceId = 0)
    {
        var (emp, charge, _) = await DbActor.NewEmployeeWithChargeAsync(fixture, name: name,
            divisionId: divisionId, crId: crId, ceId: ceId, connectionString: connectionString);
        var office = officeId ?? await DbActor.ItCellOfficeIdAsync(connectionString ?? fixture.PlainConnectionString);
        return new FakeBudgetActorSource(new BudgetActor(
            emp, charge, office, officeLevel, role, name, null, FakeBudgetActorSource.TestIp, Guid.NewGuid()));
    }
}
