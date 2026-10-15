using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update.Dtos;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.FinancialAccounting;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.DemandLines;
/// <summary>Design §8.3 expenditure / opening-figure / figure-history rows, §4.11 boundary, T-35, T-36 (DB-16).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class FiguresTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private static readonly IConfiguration Config = new ConfigurationBuilder().Build();
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"fig_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;
    private static HttpContextAccessor Http() => new() { HttpContext = new DefaultHttpContext() };

    private async Task<Response> AddAsync(AddDemandLineDto dto)
    {
        await using var ctx = _w.NewContext();
        return await new AddDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(_w.WorksProposalAId, dto), default);
    }

    private async Task<long> AddWorkLineAsync(decimal? rbe = null)
    {
        var r = await AddAsync(new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = rbe });
        r.IsSucced.Should().BeTrue(r.Message);
        return ((Response<SaveDemandLineResponseDto>)r).Data!.DemandLineId;
    }

    private async Task<Response> KeyExpenditureAsync(long lineId, decimal amount, string period = "UP_TO_DATE", string nature = "EXPENDITURE")
    {
        await using var ctx = _w.NewContext();
        return await new KeyExpenditureHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config)
            .Handle(new KeyExpenditureCommand(_w.WorksProposalAId, lineId, new KeyExpenditureDto { Nature = nature, Period = period, Amount = amount }), default);
    }

    private async Task<Response> KeyOpeningAsync(long lineId, string kind, decimal amount)
    {
        await using var ctx = _w.NewContext();
        return await new KeyOpeningFigureHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), Http(), Config)
            .Handle(new KeyOpeningFigureCommand(_w.WorksProposalAId, lineId, new KeyOpeningFigureDto { FigureKind = kind, Amount = amount }), default);
    }

    [Fact]
    public async Task BUD025_two_keyed_expenditures_are_both_kept_and_the_latest_drives_VR15()
    {
        var line = await AddWorkLineAsync();
        (await KeyExpenditureAsync(line, 500)).IsSucced.Should().BeTrue();
        (await KeyExpenditureAsync(line, 300)).IsSucced.Should().BeTrue();
        await using (var ctx = _w.NewContext())
        {
            var rows = await ctx.ExpenditureFigures.AsNoTracking().Where(e => e.WorkId == _w.WorkAId).OrderBy(e => e.ExpenditureFigureId).ToListAsync();
            rows.Select(e => e.Amount).Should().Equal(500m, 300m);
            rows.Should().OnlyContain(e => e.Source == "KEYED" && e.FinYear == 2026 && e.CreatedBy == _w.DivisionA.Actor!.EmployeeId);
        }
        await using var uctx = _w.NewContext();
        var update = new UpdateDemandLineHandler(uctx, _w.DivisionA, new BudgetActionScope(uctx, _w.DivisionA), TestServices.OfficeScope(uctx),
            TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(uctx)));
        (await update.Handle(new UpdateDemandLineCommand(_w.WorksProposalAId, line, new UpdateDemandLineDto { RbeAmount = 299 }), default))
            .MessageId.Should().Be("M-36", "the latest keyed figure, 300, is the floor");
        (await update.Handle(new UpdateDemandLineCommand(_w.WorksProposalAId, line, new UpdateDemandLineDto { RbeAmount = 400 }), default))
            .IsSucced.Should().BeTrue("400 is above the latest figure though below the first");
    }

    [Fact]
    public async Task BUD161_full_year_is_the_previous_year_and_the_nature_follows_the_category()
    {
        await using (var y = _w.NewContext())
            await y.Database.ExecuteSqlRawAsync(
                "INSERT INTO budget.fa_financial_years (fin_year_closing, start_date, end_date) VALUES (2025, '2024-04-01', '2025-03-31') ON CONFLICT DO NOTHING");
        var line = await AddWorkLineAsync();
        (await KeyExpenditureAsync(line, 100, nature: "RECEIPT")).MessageId.Should().Be("M-22");
        (await KeyExpenditureAsync(line, 900, period: "FULL_YEAR")).IsSucced.Should().BeTrue();
        await using var ctx = _w.NewContext();
        (await ctx.ExpenditureFigures.AsNoTracking().SingleAsync(e => e.WorkId == _w.WorkAId)).FinYear.Should().Be(2025);
        (await ctx.DemandLineDisplay.AsNoTracking().SingleAsync(r => r.DemandLineId == line)).ExpenditurePreviousYear.Should().Be(900);
    }

    [Fact]
    public void BUD013_validator_refuses_paise_and_a_mid_month_up_to_month()
    {
        var v = new KeyExpenditureValidator();
        Validate(v, new KeyExpenditureDto { Nature = "EXPENDITURE", Period = "UP_TO_DATE", Amount = 10.5m }).Errors.Should().Contain(e => e.ErrorCode == "M-22");
        Validate(v, new KeyExpenditureDto { Nature = "EXPENDITURE", Period = "UP_TO_MONTH", UpToMonth = new DateOnly(2026, 5, 15), Amount = 1 }).IsValid.Should().BeFalse();
        Validate(v, new KeyExpenditureDto { Nature = "EXPENDITURE", Period = "UP_TO_MONTH", UpToMonth = new DateOnly(2026, 5, 1), Amount = 1 }).IsValid.Should().BeTrue();
        Validate(v, new KeyExpenditureDto { Nature = "EXPENDITURE", Period = "UP_TO_DATE", UpToMonth = new DateOnly(2026, 5, 1), Amount = 1 }).IsValid.Should().BeFalse();
        Validate(v, new KeyExpenditureDto { Nature = "SPENT", Period = "UP_TO_DATE", Amount = 1 }).IsValid.Should().BeFalse();
        static FluentValidation.Results.ValidationResult Validate(KeyExpenditureValidator v, KeyExpenditureDto dto) =>
            v.Validate(new KeyExpenditureCommand(1, 1, dto));
    }

    [Fact]
    public async Task DB16_opening_figure_before_first_submit_is_kept_once_and_after_first_submit_is_refused()
    {
        var line = await AddWorkLineAsync();
        (await KeyOpeningAsync(line, "RBE_PREVIOUS", 10)).MessageId.Should().Be("M-22", "RBE_PREVIOUS is a Table 4 column");
        (await KeyOpeningAsync(line, "OBE_CURRENT", 1000)).IsSucced.Should().BeTrue();
        (await KeyOpeningAsync(line, "OBE_CURRENT", 1200)).IsSucced.Should().BeTrue();
        await using (var ctx = _w.NewContext())
            (await ctx.OpeningFigures.AsNoTracking().SingleAsync(o => o.WorkId == _w.WorkAId)).Amount.Should().Be(1200);
        await using (var ctx = _w.NewContext())
        {
            var t = TestServices.Transitions(ctx, _w.DivisionA);
            (await t.MoveAsync(_w.WorksProposalAId, "SUBMIT", null)).IsSucced.Should().BeTrue();
            (await t.MoveAsync(_w.WorksProposalAId, "RECALL", null)).IsSucced.Should().BeTrue();
        }
        (await KeyOpeningAsync(line, "OBE_CURRENT", 1300)).MessageId.Should().Be("M-19", "first_submitted_at is set (BUD-306)");
    }

    [Fact]
    public async Task BUD225_figure_history_lists_every_version_in_order_with_officer_ids()
    {
        var line = await AddWorkLineAsync(rbe: 100);
        await using (var uctx = _w.NewContext())
        {
            var update = new UpdateDemandLineHandler(uctx, _w.DivisionA, new BudgetActionScope(uctx, _w.DivisionA), TestServices.OfficeScope(uctx),
                TestServices.AllowAll(), Http(), Config, new DemandLineRules(new DemandLineRuleInputsLoader(uctx)));
            (await update.Handle(new UpdateDemandLineCommand(_w.WorksProposalAId, line, new UpdateDemandLineDto { RbeAmount = 120, ObeNextAmount = 50 }), default))
                .IsSucced.Should().BeTrue();
        }
        await using var ctx = _w.NewContext();
        var r = await new GetFigureHistoryHandler(ctx, _w.DivisionA, TestServices.OfficeScope(ctx)).Handle(new GetFigureHistoryQuery(_w.WorksProposalAId, line), default);
        r.IsSucced.Should().BeTrue();
        r.Data!.Select(i => (i.Figure, i.Value)).Should().Equal(("RBE", (decimal?)100m), ("OBE_NEXT", null), ("RBE", 120m), ("OBE_NEXT", 50m));
        r.Data.Should().OnlyContain(i => i.OfficerId == _w.DivisionA.Actor!.EmployeeId && i.ChargeId == _w.DivisionA.Actor.ChargeId
            && i.LevelOfficeId == _w.DivisionAOfficeId && i.ChangeSource == "ENTRY" && i.StateCode == "S-01");
        (await new GetFigureHistoryHandler(ctx, _w.DivisionB, TestServices.OfficeScope(ctx)).Handle(new GetFigureHistoryQuery(_w.WorksProposalAId, line), default))
            .MessageId.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task INT002_gateway_is_not_available()
    {
        var g = new NotAvailableFinancialAccountingGateway();
        (await g.GetExpenditureAsync(new FinancialAccountingSubject("WORK", 1, null, null), 2026, "UP_TO_DATE")).Available.Should().BeFalse();
        (await g.GetBookedAgainstEstimateCodeAsync("E1")).Should().Be(FinancialAccountingResult.Unavailable);
    }
}
