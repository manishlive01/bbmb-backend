using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.GetList;
using BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy;
using BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing;
using BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Offices;

/// <summary>
/// DDD §9 L-03 and design §8.2 rows 1-3 with the implementation-time change of 2026-10-14 (ceWings on load).
/// Each test has a fresh, fully migrated database holding only the bootstrap IT_CELL office and two CE → Circle
/// → Division hierarchies.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetOfficesTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private string _cs = string.Empty;
    private TestMasters.Hierarchy _a = default!;
    private TestMasters.Hierarchy _b = default!;
    private FakeBudgetActorSource _it = default!;

    public async Task InitializeAsync()
    {
        _cs = await fixture.CreateFreshDatabaseAsync($"off_{Guid.NewGuid():N}"[..20]);
        await fixture.MigrateToAsync(_cs, null);
        _a = await TestMasters.InsertDivisionAsync(_cs);
        _b = await TestMasters.InsertDivisionAsync(_cs);
        _it = await TestActor.CreateAsync(fixture, connectionString: _cs, name: "IT Officer");
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        await ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO budget.irr_pwr (irr_pwr_code, irr_pwr_desc, modification_date, user_code) VALUES " +
            "('I', 'Irrigation', now(), 'T'), ('P', 'Power', now(), 'T') ON CONFLICT DO NOTHING");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static LoadBudgetOfficesDto Wings(params (int CeId, string Wing)[] wings) =>
        new() { CeWings = wings.Select(w => new CeWingDto { CeId = w.CeId, WingCode = w.Wing }).ToList() };

    private async Task<LoadBudgetOfficesResponseDto> LoadAsync(LoadBudgetOfficesDto dto)
    {
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        var r = await new LoadBudgetOfficesHandler(ctx, _it, new BudgetActionScope(ctx, _it))
            .Handle(new LoadBudgetOfficesCommand(dto), default);
        r.IsSucced.Should().BeTrue(r.Message);
        return r.Data!;
    }

    private async Task<List<BudgetOffice>> OfficesAsync()
    {
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        return await ctx.BudgetOffices.AsNoTracking().OrderBy(o => o.OfficeId).ToListAsync();
    }

    private async Task<Response> SetWingAsync(long officeId, string wing)
    {
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        return await new SetOfficeWingHandler(ctx, _it, new BudgetActionScope(ctx, _it))
            .Handle(new SetOfficeWingCommand(officeId, new SetOfficeWingDto { WingCode = wing }), default);
    }

    [Fact]
    public async Task L03_partial_load_creates_the_CE_with_a_wing_and_lists_the_rest()
    {
        var r = await LoadAsync(Wings((_a.CeId, "I")));

        r.Created.Should().Be(5, "FINANCE, SECRETARIAT, CE A, Circle A, Division A");
        r.Existing.Should().Be(1, "the bootstrap IT_CELL row is reused");
        r.CesWithoutWing.Should().ContainSingle().Which.MasterId.Should().Be(_b.CeId);
        r.OfficesWithoutParent.Select(o => (o.OfficeLevel, o.MasterId)).Should().BeEquivalentTo(
            new[] { (OfficeLevel.Circle, _b.CrId), (OfficeLevel.Division, _b.DivisionId) });

        var offices = await OfficesAsync();
        offices.Count(o => o.OfficeLevel == OfficeLevel.ItCell).Should().Be(1);
        var finance = offices.Single(o => o.OfficeLevel == OfficeLevel.Finance);
        var secretariat = offices.Single(o => o.OfficeLevel == OfficeLevel.Secretariat);
        var ce = offices.Single(o => o.CeId == _a.CeId);
        var circle = offices.Single(o => o.CircleId == _a.CrId);
        var division = offices.Single(o => o.DivisionId == _a.DivisionId);
        finance.ParentOfficeId.Should().BeNull();
        secretariat.ParentOfficeId.Should().Be(finance.OfficeId);
        ce.ParentOfficeId.Should().Be(secretariat.OfficeId);
        ce.WingCode.Should().Be("I");
        circle.ParentOfficeId.Should().Be(ce.OfficeId);
        division.ParentOfficeId.Should().Be(circle.OfficeId);
        offices.Should().NotContain(o => o.CeId == _b.CeId || o.CircleId == _b.CrId || o.DivisionId == _b.DivisionId);

        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        var audited = await ctx.AuditLog.AsNoTracking()
            .Where(a => a.TableName == "office" && a.Operation == "I" && a.ActionCode == LoadBudgetOfficesHandler.ActionCode)
            .Select(a => a.RecordKey).ToListAsync();
        audited.Should().BeEquivalentTo(new[] { finance, secretariat, ce, circle, division }.Select(o => o.OfficeId.ToString()),
            "every created row has a T-51 row with action LOAD_OFFICES");
    }

    [Fact]
    public async Task L03_rerun_with_the_remaining_wing_completes_the_load_and_changes_no_existing_row()
    {
        await LoadAsync(Wings((_a.CeId, "I")));
        var before = await OfficesAsync();

        // CE A is named again with another wing: an existing row is never updated (DDD §9 L-03).
        var r = await LoadAsync(Wings((_a.CeId, "P"), (_b.CeId, "P")));

        r.Created.Should().Be(3, "CE B, Circle B, Division B");
        r.Existing.Should().Be(6);
        r.CesWithoutWing.Should().BeEmpty();
        r.OfficesWithoutParent.Should().BeEmpty();

        var after = await OfficesAsync();
        after.Should().HaveCount(before.Count + 3);
        foreach (var o in before)
            after.Single(x => x.OfficeId == o.OfficeId).Should().BeEquivalentTo(o, opt => opt.Excluding(x => x.Version));
        after.Single(o => o.CeId == _a.CeId).WingCode.Should().Be("I");
        var ceB = after.Single(o => o.CeId == _b.CeId);
        ceB.WingCode.Should().Be("P");
        after.Single(o => o.CircleId == _b.CrId).ParentOfficeId.Should().Be(ceB.OfficeId);
    }

    [Fact]
    public async Task L03_rerun_of_a_complete_load_creates_no_duplicate()
    {
        await LoadAsync(Wings((_a.CeId, "I"), (_b.CeId, "P")));
        var before = await OfficesAsync();

        var r = await LoadAsync(new LoadBudgetOfficesDto());
        r.Created.Should().Be(0);
        r.Existing.Should().Be(before.Count);
        r.CesWithoutWing.Should().BeEmpty();

        var after = await OfficesAsync();
        after.Select(o => o.OfficeId).Should().Equal(before.Select(o => o.OfficeId));
        after.GroupBy(o => o.OfficeLevel).Where(g => g.Key is "FINANCE" or "SECRETARIAT" or "IT_CELL")
            .Should().OnlyContain(g => g.Count() == 1);
    }

    [Fact]
    public async Task L03_invalid_wing_inactive_CE_and_repeated_CE_are_refused_by_the_validator()
    {
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        await ctx.Database.ExecuteSqlRawAsync($"UPDATE budget.ce_management SET status = false WHERE id = {_b.CeId}");
        var validator = new LoadBudgetOfficesValidator(ctx);

        async Task<IEnumerable<string>> Errors(LoadBudgetOfficesDto dto) =>
            (await validator.ValidateAsync(new LoadBudgetOfficesCommand(dto))).Errors.Select(e => e.ErrorMessage);

        (await Errors(Wings((_a.CeId, "ZZ")))).Should().Contain("wingCode is not a wing (irr_pwr).");
        (await Errors(Wings((_a.CeId, "")))).Should().Contain("wingCode is required.");
        (await Errors(Wings((_b.CeId, "I")))).Should().Contain("ceId is not an active CE.");
        (await Errors(Wings((999999, "I")))).Should().Contain("ceId is not an active CE.");
        (await Errors(Wings((_a.CeId, "I"), (_a.CeId, "P")))).Should().Contain("Each CE may appear once in ceWings.");
        (await Errors(Wings((_a.CeId, "I")))).Should().BeEmpty();
        (await Errors(new LoadBudgetOfficesDto())).Should().BeEmpty("the body is optional");
    }

    [Fact]
    public async Task S82_SetWing_corrects_a_CE_wing_and_refuses_other_offices_and_unknown_wings()
    {
        await LoadAsync(Wings((_a.CeId, "I")));
        var offices = await OfficesAsync();
        var ce = offices.Single(o => o.CeId == _a.CeId);
        var circle = offices.Single(o => o.CircleId == _a.CrId);

        (await SetWingAsync(circle.OfficeId, "P")).MessageId.Should().Be("M-19", "only a CE office carries a wing");
        (await SetWingAsync(ce.OfficeId, "ZZ")).MessageId.Should().Be("M-05");
        (await SetWingAsync(987654, "P")).MessageId.Should().Be("NOT_FOUND");

        (await SetWingAsync(ce.OfficeId, "P")).IsSucced.Should().BeTrue();
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
        (await ctx.BudgetOffices.AsNoTracking().SingleAsync(o => o.OfficeId == ce.OfficeId)).WingCode.Should().Be("P");
        var change = await ctx.AuditLog.AsNoTracking().SingleAsync(a =>
            a.TableName == "office" && a.RecordKey == ce.OfficeId.ToString() && a.Operation == "U" && a.ColumnName == "wing_code");
        change.ActionCode.Should().Be(SetOfficeWingHandler.ActionCode);
        change.OldValue.Should().Contain("I");
        change.NewValue.Should().Contain("P");
    }

    [Fact]
    public async Task BUD031_office_list_is_scoped_and_filtered()
    {
        await LoadAsync(Wings((_a.CeId, "I"), (_b.CeId, "P")));
        var offices = await OfficesAsync();
        var finance = offices.Single(o => o.OfficeLevel == OfficeLevel.Finance);
        var ceA = offices.Single(o => o.CeId == _a.CeId);

        async Task<PagedResponse<BBMB.BudgetService.Features.BudgetProcess.Offices.GetList.Dtos.GetBudgetOfficeListResponseDto>>
            ListAs(long officeId, string level, string? filterLevel = null, long? parentId = null)
        {
            var actor = new FakeBudgetActorSource(_it.Actor! with { OfficeId = officeId, OfficeLevel = level });
            await using var ctx = BudgetDatabaseFixture.NewBudgetContext(_cs);
            return await new GetBudgetOfficeListHandler(ctx, actor, TestServices.OfficeScope(ctx))
                .Handle(new GetBudgetOfficeListQuery(1, 50, filterLevel, parentId), default);
        }

        (await ListAs(finance.OfficeId, OfficeLevel.Finance, OfficeLevel.Ce)).Data.Select(o => o.CeId)
            .Should().BeEquivalentTo(new int?[] { _a.CeId, _b.CeId }, "Finance sees all offices");
        var fromCeA = await ListAs(ceA.OfficeId, OfficeLevel.Ce);
        fromCeA.Data.Should().HaveCount(3, "CE A sees itself, its Circle and its Division");
        fromCeA.Data.Should().OnlyContain(o => o.OfficeName != null);
        (await ListAs(ceA.OfficeId, OfficeLevel.Ce, parentId: ceA.OfficeId)).Data.Should().ContainSingle()
            .Which.CircleId.Should().Be(_a.CrId);
    }
}
