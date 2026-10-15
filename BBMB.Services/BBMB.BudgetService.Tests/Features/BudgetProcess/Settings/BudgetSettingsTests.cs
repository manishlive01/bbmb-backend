using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.BudgetProcess.Settings.Create;
using BBMB.BudgetService.Features.BudgetProcess.Settings.Create.Dtos;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Settings;

/// <summary>Design §5.3 T-17 and §8.2 settings rows (BUD-242).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetSettingsTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"set_{Guid.NewGuid():N}"[..20]);

    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateBudgetSettingCommand Cmd(string key, decimal? value, DateTimeOffset from, long? code = null) =>
        new(new CreateBudgetSettingDto { SettingKey = key, NumericValue = value, AccountCodeId = code, EffectiveFrom = from, AuthorityRef = "Order 1/2026" });

    private async Task<BBMB.BudgetService.Shared.ResponseDTOs.Response<long>> CreateAsync(CreateBudgetSettingCommand cmd)
    {
        await using var ctx = _w.NewContext();
        return await new CreateBudgetSettingHandler(ctx, _w.ItCell, new BudgetActionScope(ctx, _w.ItCell)).Handle(cmd, default);
    }

    [Fact]
    public async Task L02_seeded_values_are_in_force()
    {
        await using var ctx = _w.NewContext();
        var inForce = await new BudgetSettingsReader(ctx).GetInForceAsync(DateTimeOffset.UtcNow);
        inForce.ToDictionary(s => s.SettingKey, s => s.NumericValue).Should().BeEquivalentTo(new Dictionary<string, decimal?>
        {
            ["TS_CEILING_PCT"] = 105, ["NO_AA_LIMIT_RUPEES"] = 10000, ["AA_DEMAND_CEILING_PCT"] = 110,
            ["AMR_INCREASE_PCT"] = 5, ["UPLOAD_MAX_LINES"] = 5000,
        }, "UNFORESEEN_ACCOUNT_CODE is not seeded");
    }

    [Fact]
    public async Task BUD242_a_future_row_is_not_in_force_until_its_instant()
    {
        var from = DateTimeOffset.UtcNow.AddDays(10);
        var r = await CreateAsync(Cmd("TS_CEILING_PCT", 104, from));
        r.IsSucced.Should().BeTrue(r.Message);

        await using var ctx = _w.NewContext();
        var reader = new BudgetSettingsReader(ctx);
        (await reader.GetAsync("TS_CEILING_PCT", DateTimeOffset.UtcNow))!.NumericValue.Should().Be(105);
        (await reader.GetAsync("TS_CEILING_PCT", from.AddSeconds(-1)))!.NumericValue.Should().Be(105);
        (await reader.GetAsync("TS_CEILING_PCT", from))!.NumericValue.Should().Be(104);
        (await ctx.AuditLog.AsNoTracking().AnyAsync(a => a.TableName == "setting" && a.Operation == "I" && a.ActionCode == "SETTING_CREATE"))
            .Should().BeTrue();
    }

    [Fact]
    public void BUD242_validator_refuses_back_dating_and_malformed_values()
    {
        var v = new CreateBudgetSettingValidator();
        var future = DateTimeOffset.UtcNow.AddHours(1);
        v.Validate(Cmd("TS_CEILING_PCT", 104, DateTimeOffset.UtcNow.AddDays(-1))).Errors
            .Should().Contain(e => e.ErrorMessage == "effectiveFrom cannot be in the past.");
        v.Validate(Cmd("TS_CEILING_PCT", 1001, future)).IsValid.Should().BeFalse("percentages are 0-1000");
        v.Validate(Cmd("NO_AA_LIMIT_RUPEES", 10000.50m, future)).Errors.Should().Contain(e => e.ErrorCode == "M-22");
        v.Validate(Cmd("UPLOAD_MAX_LINES", 0, future)).IsValid.Should().BeFalse();
        v.Validate(Cmd("UNKNOWN_KEY", 1, future)).IsValid.Should().BeFalse();
        v.Validate(Cmd("AMR_INCREASE_PCT", null, future)).IsValid.Should().BeFalse("numericValue is required");
        v.Validate(Cmd("UNFORESEEN_ACCOUNT_CODE", 5, future)).IsValid.Should().BeFalse("no number for the unforeseen head");
        v.Validate(Cmd("UNFORESEEN_ACCOUNT_CODE", null, future, 7)).IsValid.Should().BeTrue();
        v.Validate(Cmd("AMR_INCREASE_PCT", 6, future)).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task S53_unforeseen_head_must_exist_and_be_in_use()
    {
        (await CreateAsync(Cmd("UNFORESEEN_ACCOUNT_CODE", null, DateTimeOffset.UtcNow.AddMinutes(5), 987654))).MessageId.Should().Be("M-05");
        (await CreateAsync(Cmd("UNFORESEEN_ACCOUNT_CODE", null, DateTimeOffset.UtcNow.AddMinutes(5), _w.AccountCodeId))).IsSucced.Should().BeTrue();
    }

    [Fact]
    public async Task DB27_UPDATE_on_budget_setting_is_refused_by_the_trigger()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using var _ = c;
        await using var cmd = new NpgsqlCommand("UPDATE budget.setting SET numeric_value = 1 WHERE setting_key = 'TS_CEILING_PCT'", c, tx);
        await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>();
    }
}
