using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Shared.Services;

/// <summary>Design §4.3 (action scope), DDD §7.1, DV-06.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetActionScopeTests(BudgetDatabaseFixture fixture)
{
    private static readonly string[] Keys =
    [
        "actor_employee_id", "actor_charge_id", "actor_office_id", "actor_role_code", "actor_name",
        "on_behalf_of_post", "client_ip", "request_id", "action_code", "justification", "no_justification",
    ];

    private static async Task<Dictionary<string, string?>> SettingsAsync(Microsoft.EntityFrameworkCore.DbContext ctx)
    {
        var result = new Dictionary<string, string?>();
        foreach (var k in Keys)
        {
            result[k] = await ctx.Database
                .SqlQueryRaw<string?>($"SELECT current_setting('budget.{k}', true) AS \"Value\"")
                .SingleAsync();
        }
        return result;
    }

    [Fact]
    public async Task DDD71_all_eleven_settings_are_visible_inside_the_transaction()
    {
        var source = await TestActor.CreateAsync(fixture, name: "Officer Scope");
        var actor = source.Actor!;
        await using var ctx = fixture.NewBudgetContext();
        var scope = new BudgetActionScope(ctx, source);

        await using (var tx = await scope.BeginAsync("TEST_ACTION", justification: "because", noJustification: false,
                         onBehalfOfPost: "FA_CAO"))
        {
            var s = await SettingsAsync(ctx);
            s["actor_employee_id"].Should().Be(actor.EmployeeId.ToString());
            s["actor_charge_id"].Should().Be(actor.ChargeId.ToString());
            s["actor_office_id"].Should().Be(actor.OfficeId.ToString());
            s["actor_role_code"].Should().Be("IT_CELL");
            s["actor_name"].Should().Be("Officer Scope");
            s["on_behalf_of_post"].Should().Be("FA_CAO");
            s["client_ip"].Should().Be("192.0.2.1");
            s["request_id"].Should().Be(actor.RequestId.ToString());
            s["action_code"].Should().Be("TEST_ACTION");
            s["justification"].Should().Be("because");
            s["no_justification"].Should().Be("");
            await tx.RollbackAsync();
        }

        // After the transaction the settings are gone (transaction-local).
        var after = await SettingsAsync(ctx);
        after.Values.Should().OnlyContain(v => string.IsNullOrEmpty(v));
    }

    [Fact]
    public async Task DV06_SettingsDoNotLeakAcrossTransactions()
    {
        // Own database (this test commits) and a one-connection pool, so the second context is guaranteed
        // to get the very connection the first one used.
        var cs = await fixture.CreateFreshDatabaseAsync("scope_dv06");
        await fixture.MigrateToAsync(cs, null);
        var pooled = new NpgsqlConnectionStringBuilder(cs) { MaxPoolSize = 1 }.ConnectionString;
        var a = await TestActor.CreateAsync(fixture, name: "Officer A", connectionString: cs);

        long id;
        int pidA;
        await using (var ctxA = BudgetDatabaseFixture.NewBudgetContext(pooled))
        {
            await using var tx = await new BudgetActionScope(ctxA, a).BeginAsync("TEST_A");
            pidA = await ctxA.Database.SqlQueryRaw<int>("SELECT pg_backend_pid() AS \"Value\"").SingleAsync();
            var office = new BudgetOffice { OfficeLevel = "FINANCE", OfficeName = "Finance (A)" };
            ctxA.BudgetOffices.Add(office);
            await ctxA.SaveChangesAsync();
            await tx.CommitAsync();
            id = office.OfficeId;
        }

        await using var ctxB = BudgetDatabaseFixture.NewBudgetContext(pooled);
        var pidB = await ctxB.Database.SqlQueryRaw<int>("SELECT pg_backend_pid() AS \"Value\"").SingleAsync();
        pidB.Should().Be(pidA, "the same pooled connection is reused");

        var office2 = await ctxB.BudgetOffices.SingleAsync(o => o.OfficeId == id);
        office2.OfficeName = "Finance (nobody)";
        var act = () => ctxB.SaveChangesAsync();
        var ex = (await act.Should().ThrowAsync<DbUpdateException>()).Which.InnerException
            .Should().BeOfType<PostgresException>().Subject;
        ex.MessageText.Should().Be("ACTOR");

        (await ctxB.AuditLog.CountAsync(r => r.TableName == "office" && r.RecordKey == id.ToString() && r.Operation == "U"))
            .Should().Be(0, "nothing is attributed to officer A");
    }

    [Fact]
    public async Task DB29_insert_inside_the_scope_is_audited_with_the_actor()
    {
        var source = await TestActor.CreateAsync(fixture, name: "Officer Audit");
        var actor = source.Actor!;
        await using var ctx = fixture.NewBudgetContext();
        await using var tx = await new BudgetActionScope(ctx, source).BeginAsync("OFFICES_LOAD");

        var office = new BudgetOffice { OfficeLevel = "FINANCE", OfficeName = "Finance (audit test)" };
        ctx.BudgetOffices.Add(office);
        await ctx.SaveChangesAsync();

        office.CreatedBy.Should().Be(actor.EmployeeId, "the [A] columns are read back from the trigger");
        office.CreatedChargeId.Should().Be(actor.ChargeId);

        var row = await ctx.AuditLog.AsNoTracking()
            .SingleAsync(r => r.TableName == "office" && r.RecordKey == office.OfficeId.ToString() && r.Operation == "I");
        row.ActorEmployeeId.Should().Be(actor.EmployeeId);
        row.ActorChargeId.Should().Be(actor.ChargeId);
        row.ActorOfficeId.Should().Be(actor.OfficeId);
        row.ActorRoleCode.Should().Be("IT_CELL");
        row.ActorName.Should().Be("Officer Audit");
        row.ClientIp!.ToString().Should().Be("192.0.2.1");
        row.RequestId.Should().Be(actor.RequestId);
        row.ActionCode.Should().Be("OFFICES_LOAD");

        await tx.RollbackAsync(); // leave the shared database unchanged
    }

    [Fact]
    public async Task Opening_a_scope_without_an_actor_is_a_coding_defect()
    {
        await using var ctx = fixture.NewBudgetContext();
        var act = () => new BudgetActionScope(ctx, FakeBudgetActorSource.None()).BeginAsync("X");
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
