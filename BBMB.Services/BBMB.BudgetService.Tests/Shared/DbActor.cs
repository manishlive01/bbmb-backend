using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.Employees.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>
/// Raw-SQL actor context for database-layer tests (DDD §7.1): the same eleven keys the application's
/// action scope sets, transaction-local.
/// </summary>
public static class DbActor
{
    public static async Task SetAsync(NpgsqlConnection conn, NpgsqlTransaction tx, Guid employeeId, Guid chargeId,
        long officeId, string role = "TEST", string action = "TEST", string name = "Test Officer")
    {
        await using var cmd = new NpgsqlCommand(
            "SELECT set_config('budget.actor_employee_id', @e, true), set_config('budget.actor_charge_id', @c, true), " +
            "set_config('budget.actor_office_id', @o, true), set_config('budget.actor_role_code', @r, true), " +
            "set_config('budget.actor_name', @n, true), set_config('budget.on_behalf_of_post', '', true), " +
            "set_config('budget.client_ip', '192.0.2.1', true), set_config('budget.request_id', gen_random_uuid()::text, true), " +
            "set_config('budget.action_code', @a, true), set_config('budget.justification', '', true), " +
            "set_config('budget.no_justification', '', true)", conn, tx);
        cmd.Parameters.AddWithValue("e", employeeId.ToString());
        cmd.Parameters.AddWithValue("c", chargeId.ToString());
        cmd.Parameters.AddWithValue("o", officeId.ToString());
        cmd.Parameters.AddWithValue("r", role);
        cmd.Parameters.AddWithValue("n", name);
        cmd.Parameters.AddWithValue("a", action);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<long> ItCellOfficeIdAsync(string connectionString)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT office_id FROM budget.office WHERE office_level = 'IT_CELL'", conn);
        return (long)(await cmd.ExecuteScalarAsync())!;
    }

    /// <summary>Inserts an employee with one charge and returns their ids.</summary>
    public static async Task<(Guid EmployeeId, Guid ChargeId, Guid UserId)> NewEmployeeWithChargeAsync(
        BudgetDatabaseFixture fixture, string projectCode = "BU", DateTime? chargeExpireTime = null,
        bool isActive = true, bool isChargeExpire = false, int? ceId = 0, int? crId = null, int? divisionId = null,
        string name = "Test Officer", string? connectionString = null)
    {
        await using var emp = connectionString is null
            ? fixture.NewEmployeeContext()
            : BudgetDatabaseFixture.NewEmployeeContext(connectionString);
        var template = await emp.Employees.AsNoTracking().FirstAsync(e => e.Id == fixture.MigrationEmployeeId);
        var e = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = Random.Shared.Next(100000, 899999),
            UserId = Guid.NewGuid(),
            TitleId = template.TitleId,
            GenderId = template.GenderId,
            NameEnglish = name,
            DateOfBirth = template.DateOfBirth,
            PhoneNumber = "0",
            Email = "x@example.invalid",
            Address = "x",
        };
        var c = new EmployeeCharge
        {
            Id = Guid.NewGuid(),
            EmployeeId = e.Id,
            ProjectCode = projectCode,
            CEId = ceId,
            CRId = crId,
            DivisionId = divisionId,
            ChargeTime = DateTime.UtcNow.AddDays(-2),
            ChargeExpireTime = chargeExpireTime,
            IsActive = isActive,
            IsChargeExpire = isChargeExpire,
        };
        emp.Employees.Add(e);
        emp.EmployeeCharges.Add(c);
        await emp.SaveChangesAsync();
        return (e.Id, c.Id, e.UserId);
    }
}
