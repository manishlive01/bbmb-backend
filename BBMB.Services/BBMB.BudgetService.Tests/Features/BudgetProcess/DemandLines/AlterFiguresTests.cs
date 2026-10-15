using System.Text.Json;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
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
/// <summary>Design §8.3 AlterFigures, §6.4 reviewer paragraph (BUD-113, BUD-122, BUD-126, BUD-127, BUD-149, BUD-317).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class AlterFiguresTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private static readonly IConfiguration Config = new ConfigurationBuilder().Build();
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"alt_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;
    private static HttpContextAccessor Http() => new() { HttpContext = new DefaultHttpContext() };
    private FakeBudgetActorSource At(long officeId, string level, string role) =>
        new(_w.ItCell.Actor! with { OfficeId = officeId, OfficeLevel = level, RoleCode = role });

    private async Task<long> AddAsync(long proposalId, AddDemandLineDto dto)
    {
        await using var ctx = _w.NewContext();
        var r = await new AddDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(proposalId, dto), default);
        r.IsSucced.Should().BeTrue(r.Message);
        return ((Response<SaveDemandLineResponseDto>)r).Data!.DemandLineId;
    }

    private async Task<Response> AlterAsync(FakeBudgetActorSource actor, long proposalId, long lineId, AlterFiguresDto dto)
    {
        await using var ctx = _w.NewContext();
        return await new AlterFiguresHandler(ctx, actor, new BudgetActionScope(ctx, actor), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new AlterFiguresCommand(proposalId, lineId, dto), default);
    }

    private async Task MoveAsync(FakeBudgetActorSource actor, long id, string action)
    {
        await using var ctx = _w.NewContext();
        var r = await TestServices.Transitions(ctx, actor).MoveAsync(id, action, null);
        r.IsSucced.Should().BeTrue($"{action}: {r.Message}");
    }

    private async Task MoveToS10Async(long id)
    {
        var ce = At(_w.CeOfficeId, "CE", "CE");
        var sec = At(_w.SecretariatOfficeId, "SECRETARIAT", "SECRETARY");
        var fin = At(_w.FinanceOfficeId, "FINANCE", "AFA_BUDGET");
        await MoveAsync(_w.DivisionA, id, "SUBMIT");
        await MoveAsync(_w.Circle, id, "OPEN");
        await MoveAsync(_w.Circle, id, "FORWARD");
        await MoveAsync(ce, id, "OPEN");
        await MoveAsync(ce, id, "FORWARD");
        await MoveAsync(sec, id, "OPEN");
        await MoveAsync(sec, id, "FORWARD_FINANCE");
        await MoveAsync(fin, id, "OPEN");
    }

    private async Task<long> WorkLineAtCircleAsync(decimal rbe = 100)
    {
        var line = await AddAsync(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = rbe, ObeNextAmount = 200 });
        await MoveAsync(_w.DivisionA, _w.WorksProposalAId, "SUBMIT");
        await MoveAsync(_w.Circle, _w.WorksProposalAId, "OPEN");
        return line;
    }

    [Fact]
    public async Task BUD113_ReviewerAlterationWritesVersionAndKeepsDivisionFigure()
    {
        var line = await WorkLineAtCircleAsync();
        var r = await AlterAsync(_w.Circle, _w.WorksProposalAId, line, new AlterFiguresDto { RbeAmount = 120, Justification = "Rates revised" });
        r.IsSucced.Should().BeTrue(r.Message);
        await using var ctx = _w.NewContext();
        var rbe = await ctx.FigureVersions.AsNoTracking().Where(v => v.DemandLineId == line && v.Figure == "RBE").OrderBy(v => v.FigureVersionId).ToListAsync();
        rbe.Select(v => (v.LevelOfficeId, v.Value, v.ChangeSource)).Should().Equal(
            (_w.DivisionAOfficeId, (decimal?)100m, "ENTRY"), (_w.CircleOfficeId, 120m, "ALTERATION"));
        rbe[1].Justification.Should().Be("Rates revised");
        rbe[1].StateCode.Should().Be("S-03");
        var display = await ctx.DemandLineDisplay.AsNoTracking().SingleAsync(d => d.DemandLineId == line);
        using var levels = JsonDocument.Parse(display.LevelFigures);
        levels.RootElement.EnumerateArray()
            .Where(e => e.GetProperty("figure").GetString() == "RBE")
            .Select(e => (e.GetProperty("level_office_id").GetInt64(), e.GetProperty("value").GetDecimal()))
            .Should().BeEquivalentTo(new[] { (_w.DivisionAOfficeId, 100m), (_w.CircleOfficeId, 120m) });
    }

    [Fact]
    public async Task BUD122_no_justification_is_ADV122_and_nothing_is_saved_continue_records_the_choice()
    {
        var line = await WorkLineAtCircleAsync();
        (await AlterAsync(_w.Circle, _w.WorksProposalAId, line, new AlterFiguresDto { RbeAmount = 130 })).MessageId.Should().Be("ADV-122");
        await using (var ctx = _w.NewContext())
        {
            (await ctx.FigureVersions.AsNoTracking().CountAsync(v => v.DemandLineId == line)).Should().Be(2, "only the Division's two entries");
            (await ctx.DemandLines.AsNoTracking().SingleAsync(l => l.DemandLineId == line)).RbeAmount.Should().Be(100);
        }
        (await AlterAsync(_w.Circle, _w.WorksProposalAId, line, new AlterFiguresDto { RbeAmount = 130, ContinueWithoutJustification = true }))
            .IsSucced.Should().BeTrue();
        await using var c2 = _w.NewContext();
        var v = await c2.FigureVersions.AsNoTracking().Where(x => x.DemandLineId == line).OrderByDescending(x => x.FigureVersionId).FirstAsync();
        v.ContinuedWithoutJustification.Should().BeTrue();
        v.Justification.Should().BeNull();
        (await c2.AuditLog.AsNoTracking().AnyAsync(a => a.TableName == "figure_version" && a.ActionCode == "CONTINUE_WITHOUT_JUSTIFICATION"))
            .Should().BeTrue();
    }

    [Fact]
    public async Task BUD127_Finance_alteration_is_recorded_on_behalf_of_FA_CAO_and_BUD317_keeps_the_floor()
    {
        var line = await AddAsync(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 600, ObeNextAmount = 200 });
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "EXPENDITURE_KEY");
        await using (c)
        {
            await using var cmd = new Npgsql.NpgsqlCommand(
                "INSERT INTO budget.expenditure_figure (subject_type, work_id, fin_year, nature, period, as_at, amount, source) " +
                $"VALUES ('WORK', {_w.WorkAId}, 2026, 'EXPENDITURE', 'UP_TO_DATE', now(), 500, 'KEYED')", c, tx);
            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        await MoveToS10Async(_w.WorksProposalAId);
        var fin = At(_w.FinanceOfficeId, "FINANCE", "AFA_BUDGET");
        (await AlterAsync(fin, _w.WorksProposalAId, line, new AlterFiguresDto { RbeAmount = 499, Justification = "Cut" })).MessageId
            .Should().Be("M-36", "VR-15 is not lifted at Finance (BUD-317)");
        (await AlterAsync(fin, _w.WorksProposalAId, line, new AlterFiguresDto { RbeAmount = 550, Justification = "Cut" })).IsSucced.Should().BeTrue();
        await using var ctx = _w.NewContext();
        var v = await ctx.FigureVersions.AsNoTracking().Where(x => x.DemandLineId == line).OrderByDescending(x => x.FigureVersionId).FirstAsync();
        v.OnBehalfOfPost.Should().Be("FA_CAO");
        v.LevelOfficeId.Should().Be(_w.FinanceOfficeId);
    }

    [Fact]
    public async Task BUD149_head_wise_figures_are_closed_at_S10()
    {
        var dna = await NewProposalAsync("DNA", _w.DivisionAOfficeId);
        var line = await AddAsync(dna, new AddDemandLineDto { LineType = "HEAD", AccountCodeId = _w.AccountCodeId, RbeAmount = 10, ObeNextAmount = 10 });
        await MoveToS10Async(dna);
        (await AlterAsync(At(_w.FinanceOfficeId, "FINANCE", "AFA_BUDGET"), dna, line, new AlterFiguresDto { RbeAmount = 5, Justification = "x" }))
            .MessageId.Should().Be("M-19");
    }

    [Fact]
    public async Task S49_the_origin_cannot_alter_and_a_reviewer_without_the_right_is_M20()
    {
        var line = await WorkLineAtCircleAsync();
        (await AlterAsync(_w.DivisionA, _w.WorksProposalAId, line, new AlterFiguresDto { RbeAmount = 1, Justification = "x" })).MessageId
            .Should().Be("M-20", "the Division is not the holder at S-03");
        await using var ctx = _w.NewContext();
        (await new AlterFiguresHandler(ctx, _w.Circle, new BudgetActionScope(ctx, _w.Circle), TestServices.OfficeScope(ctx),
                TestServices.Allow("Budget.View"), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)))
            .Handle(new AlterFiguresCommand(_w.WorksProposalAId, line, new AlterFiguresDto { RbeAmount = 1, Justification = "x" }), default))
            .MessageId.Should().Be("M-20");
    }

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
}
