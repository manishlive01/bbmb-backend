using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace BBMB.BudgetService.Tests.Shared;

public abstract class PostgresTestBase : IAsyncLifetime
{
    protected BudgetContext DbContext = default!;
    private PostgreSqlContainer _container = default!;

    public async Task InitializeAsync()
    {
        // 1️⃣ Start PostgreSQL container
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithHostname("10.147.8.83")
            .WithDatabase("budget_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();
        var connectionString = _container.GetConnectionString();
        Console.WriteLine("==================================");
        Console.WriteLine("PostgreSQL Testcontainer running");
        Console.WriteLine(connectionString);
        Console.WriteLine("==================================");

        // 2️⃣ Build DbContext options using container connection string
        var options = new DbContextOptionsBuilder<BudgetContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        // 3️⃣ Create DbContext
        DbContext = new BudgetContext(options);

        // 4️⃣ Apply migrations (this creates schema + tables)
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _container.DisposeAsync();
    }
}
