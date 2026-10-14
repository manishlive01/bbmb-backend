using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Features.PublicMasters.Genders.Entities;
using BBMB.BudgetService.Features.PublicMasters.Titles.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Testcontainers.PostgreSql;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>
/// One postgres:17 container per test collection (plan decision PD-3).
/// Mirrors the deployment order of design §11.1:
///   rule 5 - EmployeeContext is migrated before BudgetContext;
///   rule 8 - a test employee and active charge stand in for the executing IT Cell officer and are
///            passed to the BudgetContext migration through the Npgsql <c>Options</c> start-up form only;
///   rule 9 - every bbmb_gate.* is set to TEST at database level.
/// Handler tests use <see cref="PlainConnectionString"/>, which carries no migration actor.
/// </summary>
public sealed class BudgetDatabaseFixture : IAsyncLifetime
{
    public const string DatabaseName = "budget_test";

    public static readonly string[] AllGates =
    [
        "p1_state_model",
        "p1_ddd_deviations",
        "work_code_reset",
        "p5_ddd_deviations",
    ];

    private PostgreSqlContainer _container = default!;

    /// <summary>Connection string of the migrated test database, without the migration actor.</summary>
    public string PlainConnectionString { get; private set; } = string.Empty;

    /// <summary>employee.employee id of the officer standing in for the deployment executor.</summary>
    public Guid MigrationEmployeeId { get; } = Guid.NewGuid();

    /// <summary>employee.employee_charge id of that officer's active charge.</summary>
    public Guid MigrationChargeId { get; } = Guid.NewGuid();

    public const string MigrationEmployeeName = "Test Migration Officer";

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17")
            .WithDatabase(DatabaseName)
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await _container.StartAsync();

        PlainConnectionString = _container.GetConnectionString();
        await PrepareDatabaseAsync(PlainConnectionString, AllGates);
        await using var ctx = NewMigrationBudgetContext(PlainConnectionString);
        await ctx.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public BudgetContext NewBudgetContext() => NewBudgetContext(PlainConnectionString);

    public static BudgetContext NewBudgetContext(string connectionString) =>
        new(new DbContextOptionsBuilder<BudgetContext>().UseNpgsql(connectionString).Options);

    public EmployeeContext NewEmployeeContext() => NewEmployeeContext(PlainConnectionString);

    public static EmployeeContext NewEmployeeContext(string connectionString) =>
        new(new DbContextOptionsBuilder<EmployeeContext>()
            .UseNpgsql(connectionString)
            .Options);

    /// <summary>
    /// The connection string a deployment would pass to the migration run (design §11.1 rule 8):
    /// the executing officer's ids as start-up session settings.
    /// </summary>
    public string MigrationConnectionString(string connectionString) =>
        new NpgsqlConnectionStringBuilder(connectionString)
        {
            Options = $"-c bbmb_migration.actor_employee_id={MigrationEmployeeId} " +
                      $"-c bbmb_migration.actor_charge_id={MigrationChargeId}",
        }.ConnectionString;

    public BudgetContext NewMigrationBudgetContext(string connectionString) =>
        NewBudgetContext(MigrationConnectionString(connectionString));

    /// <summary>
    /// Creates an empty database in the same container, migrates EmployeeContext into it, inserts the
    /// migration officer and sets the given gates. BudgetContext is NOT migrated; use
    /// <see cref="MigrateToAsync"/>. Returns the plain connection string of the new database.
    /// </summary>
    public async Task<string> CreateFreshDatabaseAsync(string name, IEnumerable<string>? gates = null)
    {
        await using (var admin = new NpgsqlConnection(PlainConnectionString))
        {
            await admin.OpenAsync();
            await using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin);
            await cmd.ExecuteNonQueryAsync();
        }

        var cs = new NpgsqlConnectionStringBuilder(PlainConnectionString) { Database = name }.ConnectionString;
        await PrepareDatabaseAsync(cs, gates ?? AllGates);
        return cs;
    }

    /// <summary>Migrates BudgetContext of the given database to <paramref name="targetMigration"/>.</summary>
    public async Task MigrateToAsync(string connectionString, string? targetMigration, bool withMigrationActor = true)
    {
        await using var ctx = withMigrationActor
            ? NewMigrationBudgetContext(connectionString)
            : NewBudgetContext(connectionString);
        await ctx.GetService<IMigrator>().MigrateAsync(targetMigration);
    }

    private async Task PrepareDatabaseAsync(string connectionString, IEnumerable<string> gates)
    {
        // §11.1 rule 5: employee schema first.
        await using (var emp = NewEmployeeContext(connectionString))
        {
            await emp.Database.MigrateAsync();

            var gender = new Gender { GenderName = "Test" };
            var title = new Title { TitleName = "Test" };
            emp.Genders.Add(gender);
            emp.Titles.Add(title);
            await emp.SaveChangesAsync();

            emp.Employees.Add(new Employee
            {
                Id = MigrationEmployeeId,
                EmployeeCode = 900001,
                UserId = Guid.NewGuid(),
                TitleId = title.Id,
                GenderId = gender.Id,
                NameEnglish = MigrationEmployeeName,
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1980, 1, 1), DateTimeKind.Utc),
                PhoneNumber = "0000000000",
                Email = "migration.officer@example.invalid",
                Address = "Test",
            });
            emp.EmployeeCharges.Add(new EmployeeCharge
            {
                Id = MigrationChargeId,
                EmployeeId = MigrationEmployeeId,
                ProjectCode = "Budget",
                CEId = 0, // [Required] in the entity; no FK. The migration officer holds no field office.
                ChargeTime = DateTime.UtcNow.AddDays(-1),
                ChargeExpireTime = null,
                IsActive = true,
                IsChargeExpire = false,
            });
            await emp.SaveChangesAsync();
        }

        // §11.1 rule 9: gates are database-level settings; they apply to sessions opened afterwards.
        var db = new NpgsqlConnectionStringBuilder(connectionString).Database;
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        foreach (var gate in gates)
        {
            await using var cmd = new NpgsqlCommand(
                $"ALTER DATABASE \"{db}\" SET \"bbmb_gate.{gate}\" = 'TEST'", conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
