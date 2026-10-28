using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetById;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Rules;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.FinancialAccounting;
using BBMB.BudgetService.Shared.Validators;
using BBMB.BudgetService.Tests.Features.BudgetProcess.AdminApprovals;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.TechnicalSanctions;
/// <summary>Design §8.4 technical-sanction rows (F-06; VR-02, VR-10, VR-11; BUD-069 to BUD-076, BUD-271 to BUD-274).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class TechnicalSanctionTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private long _approvalId;
    private long _att;
    public async Task InitializeAsync()
    {
        _w = await SpineWorld.CreateAsync(fixture, $"tsn_{Guid.NewGuid():N}"[..20]);
        _att = await CreateAdminApprovalTests.EvidenceAsync(_w, _w.DivisionA);
        var aa = await CreateAdminApprovalTests.CreateAsync(_w, CreateAdminApprovalTests.Dto(_w, _att, workId: _w.WorkAId, amount: 1000));
        aa.IsSucced.Should().BeTrue(aa.Message);
        _approvalId = aa.Data!.AdminApprovalId;
    }
    public Task DisposeAsync() => Task.CompletedTask;

    private sealed class BookedGateway(decimal booked) : IFinancialAccountingGateway
    {
        public Task<FinancialAccountingResult> GetExpenditureAsync(FinancialAccountingSubject subject, int finYear, string period, CancellationToken ct = default) =>
            Task.FromResult(FinancialAccountingResult.Unavailable);
        public Task<FinancialAccountingResult> GetBookedAgainstEstimateCodeAsync(string estimateCode, CancellationToken ct = default) =>
            Task.FromResult(new FinancialAccountingResult(true, booked));
    }

    private static TechnicalSanctionRules Rules(Infrastructure.Persistence.BudgetContexts.BudgetContext ctx, IFinancialAccountingGateway? fa = null) =>
        new(ctx, new BudgetSettingsReader(ctx), fa ?? new NotAvailableFinancialAccountingGateway());

    private CreateTechnicalSanctionDto Aa(decimal amount, string number) => new()
    {
        Tab = "AA", AdminApprovalId = _approvalId, NameOfEstimate = "Estimate", TsNumber = number,
        TsDate = RupeeRuleExtensions.TodayIst(), TsAmount = amount, TsCopyAttachmentId = _att,
    };

    private CreateTechnicalSanctionDto Amr(decimal amount, string number, int? workId = null) => new()
    {
        Tab = "AMR", WorkId = workId, NewAmrWork = workId is null ? new NewAmrWorkDto { NameOfWork = "Annual repairs", AccountCodeId = _w.AccountCodeId } : null,
        NameOfEstimate = "AM&R estimate", TsNumber = number, TsDate = RupeeRuleExtensions.TodayIst(), TsAmount = amount, TsCopyAttachmentId = _att,
    };

    private async Task<Response<long>> CreateAsync(CreateTechnicalSanctionDto dto, FakeBudgetActorSource? actor = null)
    {
        actor ??= _w.DivisionA;
        await using var ctx = _w.NewContext();
        return await new CreateTechnicalSanctionHandler(ctx, actor, new BudgetActionScope(ctx, actor), Rules(ctx), new WorkRules(ctx))
            .Handle(new CreateTechnicalSanctionCommand(dto), default);
    }

    private async Task<Response<int>> ReviseAsync(long id, decimal amount, string number, IFinancialAccountingGateway? fa = null)
    {
        await using var ctx = _w.NewContext();
        return await new ReviseTechnicalSanctionHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA),
                TestServices.OfficeScope(ctx), Rules(ctx, fa))
            .Handle(new ReviseTechnicalSanctionCommand(id, new ReviseTechnicalSanctionDto
            {
                NameOfEstimate = "Estimate (revised)", TsNumber = number, TsDate = RupeeRuleExtensions.TodayIst(),
                TsAmount = amount, TsCopyAttachmentId = _att,
            }), default);
    }

    private async Task<int> SanctionCountAsync()
    {
        await using var ctx = _w.NewContext();
        return await ctx.TechnicalSanctions.CountAsync();
    }

    private async Task RecordEstimateCodeAsync(long sanctionId, string code)
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell, "FA_ESTIMATE_CODE");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand($"SELECT budget.fn_record_estimate_code({sanctionId}, '{code}')", c, tx);
        await cmd.ExecuteNonQueryAsync();
        await tx.CommitAsync();
    }

    [Fact]
    public async Task VR02_exactly_105_percent_is_accepted_and_one_rupee_more_is_refused_M06_by_the_application()
    {
        (await CreateAsync(Aa(1050, "TS/1"))).IsSucced.Should().BeTrue("1050 = 105% of 1000");
        var refused = await CreateAsync(Aa(1, "TS/2"));
        refused.MessageId.Should().Be(BudgetMessages.M06);
        (await SanctionCountAsync()).Should().Be(1, "the pre-check refuses before any row is written (DB-09 never reached)");
    }

    [Fact]
    public async Task VR02_single_sanction_above_105_percent_is_refused_M06()
    {
        (await CreateAsync(Aa(1051, "TS/1"))).MessageId.Should().Be(BudgetMessages.M06);
        (await SanctionCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task VR02_revision_counts_the_sanctions_own_current_version_once()
    {
        var id = (await CreateAsync(Aa(1000, "TS/1"))).Data;
        (await ReviseAsync(id, 1050, "TS/1")).IsSucced.Should().BeTrue("1000 is replaced, not added");
        (await ReviseAsync(id, 1051, "TS/1")).MessageId.Should().Be(BudgetMessages.M06);
    }

    [Fact]
    public async Task BUD069_AA_tab_takes_the_work_from_the_approval()
    {
        var id = (await CreateAsync(Aa(500, "TS/1"))).Data;
        await using var ctx = _w.NewContext();
        var got = await new GetTechnicalSanctionByIdHandler(ctx, _w.DivisionA, TestServices.OfficeScope(ctx))
            .Handle(new GetTechnicalSanctionByIdQuery(id), default);
        got.Data!.WorkId.Should().Be(_w.WorkAId);
        got.Data.AdminApprovalId.Should().Be(_approvalId);
        got.Data.Versions.Should().ContainSingle().Which.TsCopyUrl.Should().Be($"/api/budget-attachments/{_att}");
        await using var ctx2 = _w.NewContext();
        (await new GetTechnicalSanctionByIdHandler(ctx2, _w.DivisionB, TestServices.OfficeScope(ctx2))
            .Handle(new GetTechnicalSanctionByIdQuery(id), default)).MessageId.Should().Be(BudgetMessages.NotFound);
    }

    [Fact]
    public async Task BUD274_second_sanction_for_an_AMR_work_is_refused_M06_and_the_new_work_is_TS_ENTRY()
    {
        var first = await CreateAsync(Amr(5000, "TS/A1"));
        first.IsSucced.Should().BeTrue(first.Message);
        int workId;
        await using (var ctx = _w.NewContext())
        {
            var t = await ctx.TechnicalSanctions.SingleAsync(x => x.TechnicalSanctionId == first.Data);
            workId = t.WorkId;
            var work = await ctx.Works.SingleAsync(w => w.Id == workId);
            work.EnteredVia.Should().Be(WorkEnteredVia.TsEntry);
            work.WorkClass.Should().Be(WorkClass.Amr);
        }
        (await CreateAsync(Amr(100, "TS/A2", workId))).MessageId.Should().Be(BudgetMessages.M06);
    }

    [Fact]
    public async Task VR11_revise_states_it_was_not_applied_while_Financial_Accounting_is_unavailable()
    {
        var id = (await CreateAsync(Amr(5000, "TS/A1"))).Data;
        await RecordEstimateCodeAsync(id, "EST-001");
        var r = await ReviseAsync(id, 4000, "TS/A1");
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data.Should().Be(2);
        r.Message.Should().Contain("VR-11 not applied").And.Contain("Financial Accounting is not available");
    }

    [Fact]
    public async Task VR11_with_Financial_Accounting_available_a_revision_below_booked_is_M31()
    {
        var id = (await CreateAsync(Amr(5000, "TS/A1"))).Data;
        await RecordEstimateCodeAsync(id, "EST-002");
        (await ReviseAsync(id, 1999, "TS/A1", new BookedGateway(2000))).MessageId.Should().Be(BudgetMessages.M31);
        (await ReviseAsync(id, 2000, "TS/A1", new BookedGateway(2000))).IsSucced.Should().BeTrue();
    }

    [Fact]
    public void Validator_refuses_adminApprovalId_on_the_AMR_tab_and_a_missing_copy_M07()
    {
        var v = new CreateTechnicalSanctionValidator();
        var amr = Amr(100, "TS/1");
        amr.AdminApprovalId = 5;
        v.Validate(new CreateTechnicalSanctionCommand(amr)).IsValid.Should().BeFalse();
        var aaWithWork = Aa(100, "TS/1");
        aaWithWork.WorkId = 3;
        v.Validate(new CreateTechnicalSanctionCommand(aaWithWork)).IsValid.Should().BeFalse();
        var noCopy = Aa(100, "TS/1");
        noCopy.TsCopyAttachmentId = null;
        v.Validate(new CreateTechnicalSanctionCommand(noCopy)).Errors.Should().Contain(e => e.ErrorCode == BudgetMessages.M07);
        v.Validate(new CreateTechnicalSanctionCommand(Aa(100, "TS/1"))).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task DB07_TS_number_reused_in_the_Division_is_M19_and_another_Division_sees_no_approval()
    {
        (await CreateAsync(Amr(100, "TS/X"))).IsSucced.Should().BeTrue();
        (await CreateAsync(Aa(100, "TS/X"))).MessageId.Should().Be(BudgetMessages.M19);
        (await CreateAsync(Aa(100, "TS/Y"), _w.DivisionB)).MessageId.Should().Be(BudgetMessages.NotFound);
        (await CreateAsync(Aa(100, "TS/Y"), _w.Circle)).MessageId.Should().Be(BudgetMessages.M20);
    }

    [Fact]
    public void Routes_carry_F06_Enter_and_Budget_View()
    {
        EndpointCatalog.Find("POST", "/api/technical-sanctions")!.Policies.Should().Equal("BudgetRight:F06.Enter");
        EndpointCatalog.Find("POST", "/api/technical-sanctions/{id:long}/revisions")!.Policies.Should().Equal("BudgetRight:F06.Enter");
        EndpointCatalog.Find("POST", "/api/technical-sanctions/list")!.Policies.Should().Equal("BudgetRight:Budget.View");
        EndpointCatalog.Find("GET", "/api/technical-sanctions/{id:long}")!.Policies.Should().Equal("BudgetRight:Budget.View");
    }
}
