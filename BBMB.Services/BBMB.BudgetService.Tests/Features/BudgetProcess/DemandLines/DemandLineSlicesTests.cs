using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.MarkDeleted;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.DemandLines;
/// <summary>Design §8.3 demand-line rows, §4.9 scope rule, §4.10 lease, §6.4 Save moment.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class DemandLineSlicesTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private static readonly IConfiguration Config = new ConfigurationBuilder().Build();
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"dls_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;
    private static HttpContextAccessor Http() => new() { HttpContext = new DefaultHttpContext() };

    private async Task<Response> AddAsync(FakeBudgetActorSource actor, AddDemandLineDto dto, long? proposalId = null, FakeAuthorizationService? auth = null)
    {
        await using var ctx = _w.NewContext();
        return await new AddDemandLineHandler(ctx, actor, new BudgetActionScope(ctx, actor), TestServices.OfficeScope(ctx),
                auth ?? TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(proposalId ?? _w.WorksProposalAId, dto), default);
    }

    private async Task<Response> UpdateAsync(FakeBudgetActorSource actor, long lineId, UpdateDemandLineDto dto)
    {
        await using var ctx = _w.NewContext();
        return await new UpdateDemandLineHandler(ctx, actor, new BudgetActionScope(ctx, actor), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new UpdateDemandLineCommand(_w.WorksProposalAId, lineId, dto), default);
    }

    private async Task<Response> DeleteAsync(FakeBudgetActorSource actor, long lineId)
    {
        await using var ctx = _w.NewContext();
        return await new MarkDemandLineDeletedHandler(ctx, actor, new BudgetActionScope(ctx, actor), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config)
            .Handle(new MarkDemandLineDeletedCommand(_w.WorksProposalAId, lineId), default);
    }

    private static SaveDemandLineResponseDto Saved(Response r)
    {
        r.IsSucced.Should().BeTrue(r.Message);
        return ((Response<SaveDemandLineResponseDto>)r).Data!;
    }

    private static AddDemandLineDto NewWorkLine(string name, decimal? rbe = 100, decimal? obe = 200, string workClass = "AA", long accountCodeId = 0) =>
        new() { LineType = "WORK", NewWork = new NewWorkDto { NameOfWork = name, AccountCodeId = accountCodeId, WorkClass = workClass }, RbeAmount = rbe, ObeNextAmount = obe };

    private async Task<long> NewProposalAsync(string category, long officeId)
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS");
        var p = new BudgetProposal { CycleId = _w.CycleId, Category = category, OriginOfficeId = officeId, HolderOfficeId = officeId, StateCode = "S-01" };
        ctx.BudgetProposals.Add(p);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
        return p.ProposalId;
    }

    [Fact]
    public async Task BUD095_add_at_S02_is_M19()
    {
        await using (var ctx = _w.NewContext())
            (await TestServices.Transitions(ctx, _w.DivisionA).MoveAsync(_w.WorksProposalAId, "SUBMIT", null)).IsSucced.Should().BeTrue();
        (await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 1 }))
            .MessageId.Should().Be("M-19");
    }

    [Fact]
    public async Task BUD031_add_by_another_office_is_NOT_FOUND_when_invisible_and_M20_when_visible()
    {
        var dto = new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 1 };
        (await AddAsync(_w.DivisionB, dto)).MessageId.Should().Be("NOT_FOUND", "Division B cannot see Division A's proposal");
        (await AddAsync(_w.Circle, dto)).MessageId.Should().Be("M-20", "the Circle sees the proposal but is not its origin");
        (await AddAsync(_w.DivisionA, dto, auth: TestServices.Allow("Budget.View"))).MessageId
            .Should().Be("M-20", "F07.Enter is required for a WORKS demand");
        await using var c = _w.NewContext();
        (await c.DemandLines.AsNoTracking().CountAsync(l => l.ProposalId == _w.WorksProposalAId)).Should().Be(0);
    }

    [Fact]
    public async Task BUD104_duplicate_work_is_M25_and_nothing_is_written()
    {
        Saved(await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 10 }));
        var dup = await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 20 });
        dup.MessageId.Should().Be("M-25");
        ((Response<RuleFailuresDto>)dup).Data!.Failures.Should().ContainSingle(f => f.Field == "workId" && f.MessageId == "M-25");
        await using var c = _w.NewContext();
        (await c.DemandLines.AsNoTracking().CountAsync(l => l.ProposalId == _w.WorksProposalAId)).Should().Be(1);
    }

    [Fact]
    public async Task DV19_new_work_enters_via_DEMAND_ENTRY_with_its_Division_and_the_actor_as_creator()
    {
        var saved = Saved(await AddAsync(_w.DivisionA, NewWorkLine("New canal lining", accountCodeId: _w.AccountCodeId)));
        await using var c = _w.NewContext();
        var line = await c.DemandLines.AsNoTracking().SingleAsync(l => l.DemandLineId == saved.DemandLineId);
        var work = await c.Works.AsNoTracking().SingleAsync(w => w.Id == line.WorkId);
        work.EnteredVia.Should().Be("DEMAND_ENTRY");
        work.PreDdd.Should().BeFalse();
        work.WorkClass.Should().Be("AA");
        work.DivisionId.Should().Be(_w.HierarchyA.DivisionId);
        work.CreatedBy.Should().Be(_w.DivisionA.Actor!.EmployeeId);
        line.AccountCodeId.Should().Be(_w.AccountCodeId);
        line.FloorCheckUnchecked.Should().BeTrue("no expenditure is available for a new work (VR-15)");
        // DV-19 (design §4.6): a work created with its demand line (DEMAND_ENTRY) carries NO code after the
        // P4 reset — a code is generated only on the four allocation occasions (BUD-232).
        work.WorkCode.Should().BeNull();
    }

    [Fact]
    public async Task BUD079_serials_are_allotted_1_2_3_in_order()
    {
        var a = Saved(await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId }));
        var b = Saved(await AddAsync(_w.DivisionA, NewWorkLine("Second", accountCodeId: _w.AccountCodeId)));
        var u = Saved(await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "UNFORESEEN", UnforeseenName = "Flood works" }));
        new[] { a.SerialNo, b.SerialNo, u.SerialNo }.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task DB14_updating_rbe_writes_one_T31_row_ENTRY_and_mark_deleted_keeps_row_and_versions()
    {
        var saved = Saved(await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 100, ObeNextAmount = 200 }));
        Saved(await UpdateAsync(_w.DivisionA, saved.DemandLineId, new UpdateDemandLineDto { RbeAmount = 150, ObeNextAmount = 200 }));
        await using (var c = _w.NewContext())
        {
            var versions = await c.FigureVersions.AsNoTracking().Where(v => v.DemandLineId == saved.DemandLineId)
                .OrderBy(v => v.FigureVersionId).ToListAsync();
            versions.Where(v => v.Figure == "RBE").Select(v => v.Value).Should().Equal(100m, 150m);
            versions.Where(v => v.Figure == "OBE_NEXT").Should().ContainSingle("OBE did not change");
            versions.Should().OnlyContain(v => v.ChangeSource == "ENTRY" && v.LevelOfficeId == _w.DivisionAOfficeId);
        }
        var del = await DeleteAsync(_w.DivisionA, saved.DemandLineId);
        del.IsSucced.Should().BeTrue(del.Message);
        await using (var c = _w.NewContext())
        {
            var line = await c.DemandLines.AsNoTracking().SingleAsync(l => l.DemandLineId == saved.DemandLineId);
            line.IsDeleted.Should().BeTrue();
            line.DeletedBy.Should().Be(_w.DivisionA.Actor!.EmployeeId, "set by fn_demand_line_rules from the actor context");
            (await c.FigureVersions.AsNoTracking().CountAsync(v => v.DemandLineId == saved.DemandLineId)).Should().Be(3);
            (await c.Works.AsNoTracking().AnyAsync(w => w.Id == _w.WorkAId)).Should().BeTrue("BUD-320: the work is untouched");
        }
        (await UpdateAsync(_w.DivisionA, saved.DemandLineId, new UpdateDemandLineDto { RbeAmount = 1 })).MessageId
            .Should().Be("M-19", "a deleted line is kept, not edited");
        (await DeleteAsync(_w.DivisionA, saved.DemandLineId)).IsSucced.Should().BeTrue("idempotent");
        // The deleted work may be demanded again on a new line (the partial UK excludes deleted lines).
        Saved(await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId }));
    }

    [Fact]
    public async Task DB01_DNA_proposal_with_lineType_WORK_is_refused_by_the_validator()
    {
        var dna = await NewProposalAsync("DNA", _w.DivisionAOfficeId);
        await using var ctx = _w.NewContext();
        var v = new AddDemandLineValidator(ctx, _w.DivisionA, TestServices.OfficeScope(ctx));
        (await v.ValidateAsync(new AddDemandLineCommand(dna, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId })))
            .Errors.Should().Contain(e => e.PropertyName == "lineType");
        (await v.ValidateAsync(new AddDemandLineCommand(dna, new AddDemandLineDto { LineType = "HEAD", AccountCodeId = _w.AccountCodeId })))
            .IsValid.Should().BeTrue();
        (await v.ValidateAsync(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto { LineType = "HEAD", AccountCodeId = _w.AccountCodeId })))
            .Errors.Should().Contain(e => e.PropertyName == "lineType");
        (await v.ValidateAsync(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto
            { LineType = "WORK", WorkId = _w.WorkAId, NewWork = new NewWorkDto { NameOfWork = "x", AccountCodeId = 1, WorkClass = "AA" } })))
            .IsValid.Should().BeFalse("exactly one of workId and newWork");
        (await v.ValidateAsync(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 10.5m })))
            .Errors.Should().Contain(e => e.ErrorCode == "M-22");
        (await v.ValidateAsync(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = -500 })))
            .IsValid.Should().BeTrue("negatives are allowed (BUD-088)");
    }

    [Fact]
    public async Task DB04_work_of_another_Division_is_M05_on_workId()
    {
        var r = await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkBId });
        r.MessageId.Should().Be("M-05");
        ((Response<RuleFailuresDto>)r).Data!.Failures.Should().ContainSingle(f => f.Field == "workId");
    }

    [Fact]
    public async Task VR15_save_below_keyed_expenditure_is_M36()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "EXPENDITURE_KEY");
        await using (c)
        {
            await using var cmd = new Npgsql.NpgsqlCommand(
                "INSERT INTO budget.expenditure_figure (subject_type, work_id, fin_year, nature, period, as_at, amount, source) " +
                $"VALUES ('WORK', {_w.WorkAId}, 2026, 'EXPENDITURE', 'UP_TO_DATE', current_date, 500, 'KEYED')", c, tx);
            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        (await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 499 })).MessageId.Should().Be("M-36");
        Saved(await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 500 }));
    }

    [Fact]
    public async Task BUD015_another_officers_live_lease_is_M23()
    {
        await using (var ctx = _w.NewContext())
        await using (var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("LEASE"))
        {
            var p = await ctx.BudgetProposals.SingleAsync(x => x.ProposalId == _w.WorksProposalAId);
            p.EditLeaseBy = _w.ItCell.Actor!.EmployeeId; // another officer of record holds it
            p.EditLeaseExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
        }
        var other = new FakeBudgetActorSource(_w.DivisionA.Actor! with { EmployeeId = Guid.NewGuid() });
        (await AddAsync(other, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId })).MessageId.Should().Be("M-23");
    }

    [Fact]
    public async Task BUD305_list_filters_by_tab_hides_deleted_and_names_the_year_in_every_heading()
    {
        var a = Saved(await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 7 }));
        Saved(await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "UNFORESEEN", UnforeseenName = "Flood works" }));
        var amr = Saved(await AddAsync(_w.DivisionA, NewWorkLine("AMR work", workClass: "AMR", accountCodeId: _w.AccountCodeId)));
        (await DeleteAsync(_w.DivisionA, amr.DemandLineId)).IsSucced.Should().BeTrue();
        await using var ctx = _w.NewContext();
        var h = new GetDemandLinesByProposalHandler(ctx, _w.DivisionA, TestServices.OfficeScope(ctx));
        var all = (await h.Handle(new GetDemandLinesByProposalQuery(_w.WorksProposalAId, null, false, 1, 50), default)).Data!;
        all.Lines.Select(l => l.SerialNo).Should().Equal(1, 2);
        all.Lines[0].LevelFigures.Should().Contain(f => f.Figure == "RBE" && f.Value == 7 && f.LevelOfficeId == _w.DivisionAOfficeId);
        all.Headings["rbeAmount"].Should().Be("Revised estimate 2025-26");
        all.Headings["obeNextAmount"].Should().Be("Original budget estimate 2026-27");
        all.Headings.Values.Should().OnlyContain(v => v.Contains("20"));
        (await h.Handle(new GetDemandLinesByProposalQuery(_w.WorksProposalAId, "AA", false, 1, 50), default)).Data!.Lines
            .Should().ContainSingle(l => l.DemandLineId == a.DemandLineId);
        (await h.Handle(new GetDemandLinesByProposalQuery(_w.WorksProposalAId, "AMR", true, 1, 50), default)).Data!.Lines
            .Should().ContainSingle(l => l.IsDeleted);
        var hB = new GetDemandLinesByProposalHandler(ctx, _w.DivisionB, TestServices.OfficeScope(ctx));
        (await hB.Handle(new GetDemandLinesByProposalQuery(_w.WorksProposalAId, null, false, 1, 50), default)).MessageId.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task BUD275_amr_options_are_empty_in_the_first_cycle_and_a_predecessor_outside_them_is_M22()
    {
        await using (var ctx = _w.NewContext())
            (await new GetAmrPredecessorOptionsHandler(ctx, _w.DivisionA, TestServices.OfficeScope(ctx))
                .Handle(new GetAmrPredecessorOptionsQuery(_w.WorksProposalAId), default)).Data.Should().BeEmpty();
        var dto = NewWorkLine("Continuing AMR", workClass: "AMR", accountCodeId: _w.AccountCodeId);
        dto.PrecedingAmrWorkId = _w.WorkAId;
        (await AddAsync(_w.DivisionA, dto)).MessageId.Should().Be("M-22");
    }
}
