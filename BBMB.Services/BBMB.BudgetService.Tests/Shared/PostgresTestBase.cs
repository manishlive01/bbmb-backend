using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>
/// Per-test-class database (one postgres:17 container each). Prepared exactly as
/// <see cref="BudgetDatabaseFixture"/> prepares the shared one, because the budget migrations need what a
/// deployment needs (design §11.1 rules 5, 8, 9): EmployeeContext first, the executing officer passed through
/// the migration connection's Options, every bbmb_gate.* set. <see cref="DbContext"/> uses the plain
/// connection string, as an application session does.
/// </summary>
public abstract class PostgresTestBase : IAsyncLifetime
{
    protected BudgetContext DbContext = default!;
    protected BudgetDatabaseFixture Database { get; } = new();

    public async Task InitializeAsync()
    {
        await Database.InitializeAsync();
        DbContext = Database.NewBudgetContext();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await Database.DisposeAsync();
    }
}
