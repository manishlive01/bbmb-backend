using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit;
using BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Settings.GetDelegationLimits;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Settings;
/// <summary>Design §5.3 T-19 and §8.2 delegation row (BUD-055 input; DB-27).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class DelegationLimitTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"dlg_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateDelegationLimitCommand Cmd(string level, decimal? max, DateOnly from) =>
        new(new CreateDelegationLimitDto { AuthorityLevel = level, MaxAmount = max, EffectiveFrom = from, SourceRef = "R-06 order 1" });

    private async Task<Response<long>> CreateAsync(CreateDelegationLimitCommand cmd, FakeBudgetActorSource? actor = null)
    {
        actor ??= _w.ItCell;
        await using var ctx = _w.NewContext();
        return await new CreateDelegationLimitHandler(ctx, actor, new BudgetActionScope(ctx, actor)).Handle(cmd, default);
    }

    [Fact]
    public async Task BUD055_limit_in_force_is_the_latest_row_on_or_before_the_date()
    {
        (await CreateAsync(Cmd("EE", 500_000, new DateOnly(2025, 4, 1)))).IsSucced.Should().BeTrue();
        (await CreateAsync(Cmd("EE", 1_000_000, new DateOnly(2026, 4, 1)))).IsSucced.Should().BeTrue();
        (await CreateAsync(Cmd("CE", null, new DateOnly(2025, 4, 1)))).IsSucced.Should().BeTrue();
        await using var ctx = _w.NewContext();
        var reader = new DelegationLimitReader(ctx);
        (await reader.GetInForceAsync("EE", new DateOnly(2025, 3, 31))).Should().BeNull("no limit before the first order");
        (await reader.GetInForceAsync("EE", new DateOnly(2026, 3, 31)))!.MaxAmount.Should().Be(500_000);
        (await reader.GetInForceAsync("EE", new DateOnly(2026, 4, 1)))!.MaxAmount.Should().Be(1_000_000, "in force from its own date");
        (await reader.GetInForceAsync("CE", new DateOnly(2026, 4, 1)))!.MaxAmount.Should().BeNull("a null amount is no limit");

        var all = await new GetDelegationLimitsHandler(reader).Handle(new GetDelegationLimitsQuery(new DateOnly(2026, 3, 31)), default);
        all.Data!.Should().HaveCount(3);
        all.Data!.Where(d => d.InForce).Select(d => (d.AuthorityLevel, d.MaxAmount))
            .Should().BeEquivalentTo([("CE", (decimal?)null), ("EE", (decimal?)500_000)]);
    }

    [Fact]
    public async Task DB27_update_and_delete_of_a_delegation_limit_are_refused()
    {
        var id = (await CreateAsync(Cmd("SE", 200_000, new DateOnly(2026, 4, 1)))).Data;
        foreach (var sql in new[]
                 {
                     $"UPDATE budget.delegation_limit SET max_amount = 1 WHERE delegation_limit_id = {id}",
                     $"DELETE FROM budget.delegation_limit WHERE delegation_limit_id = {id}",
                 })
        {
            var (c, tx) = await _w.BeginAsync(_w.ItCell);
            await using var _ = c;
            await using var cmd = new NpgsqlCommand(sql, c, tx);
            (await cmd.Invoking(x => x.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>())
                .Which.MessageText.Should().Be("FORBIDDEN", sql);
        }
    }

    [Fact]
    public void Validator_refuses_maxAmount_zero_paise_and_an_unknown_level()
    {
        var v = new CreateDelegationLimitValidator();
        v.Validate(Cmd("EE", 0, new DateOnly(2026, 4, 1))).IsValid.Should().BeFalse("maxAmount > 0 where set");
        v.Validate(Cmd("EE", 100.50m, new DateOnly(2026, 4, 1))).Errors.Should().Contain(e => e.ErrorCode == BudgetMessages.M22);
        v.Validate(Cmd("AE", 100, new DateOnly(2026, 4, 1))).IsValid.Should().BeFalse("level in the four");
        v.Validate(Cmd("EE", null, default)).IsValid.Should().BeFalse("effectiveFrom required");
        v.Validate(Cmd("SECRETARY", null, new DateOnly(2026, 4, 1))).IsValid.Should().BeTrue("null = no limit");
    }

    [Fact]
    public async Task DB_check_refuses_maxAmount_zero_written_past_the_validator()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.delegation_limit (authority_level, max_amount, effective_from, source_ref) VALUES ('EE', 0, current_date, 'R-06')", c, tx);
        (await cmd.Invoking(x => x.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be("23514");
    }

    [Fact]
    public async Task Same_level_and_date_twice_is_refused_M19_and_no_actor_is_M20()
    {
        (await CreateAsync(Cmd("EE", 100, new DateOnly(2026, 4, 1)))).IsSucced.Should().BeTrue();
        (await CreateAsync(Cmd("EE", 200, new DateOnly(2026, 4, 1)))).MessageId.Should().Be(BudgetMessages.M19);
        (await CreateAsync(Cmd("EE", 200, new DateOnly(2026, 5, 1)), FakeBudgetActorSource.None())).MessageId.Should().Be(BudgetMessages.M20);
        BudgetMessages.UniqueConstraintMessageFor("IX_delegation_limit_authority_level_effective_from").Should().Be(BudgetMessages.M19);
    }

    [Fact]
    public void Routes_carry_Settings_Manage_and_Budget_View()
    {
        EndpointCatalog.Find("POST", "/api/budget-settings/delegation-limits")!.Policies.Should().Equal("BudgetRight:Settings.Manage");
        EndpointCatalog.Find("GET", "/api/budget-settings/delegation-limits")!.Policies.Should().Equal("BudgetRight:Budget.View");
        EndpointCatalog.All.Where(e => e.Pattern.StartsWith("/api/budget-settings/delegation-limits", StringComparison.Ordinal))
            .SelectMany(e => e.Methods).Should().NotContain(["PUT", "DELETE", "PATCH"], "T-19 is append-only");
    }
}
