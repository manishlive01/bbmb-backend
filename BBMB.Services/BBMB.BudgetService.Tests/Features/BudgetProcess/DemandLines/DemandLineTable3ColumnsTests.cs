using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetById;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.DemandLines;

/// <summary>Table 3 A&amp;A / TS columns as ToRow maps them from V-04 (no database).</summary>
public sealed class DemandLineTable3ToRowTests
{
    [Fact]
    public void T3_ToRow_maps_AA_and_TS_fields_and_nameOfWork_prefers_the_AA_name()
    {
        var row = new DemandLineDisplayRow
        {
            DemandLineId = 1, SerialNo = 1, LineType = "WORK", WorkId = 5, WorkDesc = "Master name",
            AdminApprovalId = 42, AaNameOfWork = "A&A name", AaReferenceNo = "AA/7", AaDateOfApproval = new DateOnly(2025, 6, 30),
            AaAmountSanctioned = 1000m, TsCount = 2, TsAmountToDate = 750m,
        };
        var dto = GetDemandLinesByProposalHandler.ToRow(row);
        dto.AdminApprovalId.Should().Be(42);
        dto.AaNameOfWork.Should().Be("A&A name");
        dto.AaReferenceNo.Should().Be("AA/7");
        dto.AaDateOfApproval.Should().Be(new DateOnly(2025, 6, 30));
        dto.AaAmountSanctioned.Should().Be(1000m);
        dto.TsCount.Should().Be(2);
        dto.TsAmountToDate.Should().Be(750m);
        dto.NameOfWork.Should().Be("A&A name");
        dto.WorkDesc.Should().Be("Master name", "workDesc is kept as it is");
        dto.IsNameLocked.Should().BeTrue();
    }

    [Fact]
    public void T3_ToRow_without_AA_falls_back_to_workDesc_unlocked_with_tsCount_zero()
    {
        var dto = GetDemandLinesByProposalHandler.ToRow(new DemandLineDisplayRow { LineType = "WORK", WorkId = 5, WorkDesc = "Master name" });
        dto.NameOfWork.Should().Be("Master name");
        dto.IsNameLocked.Should().BeFalse();
        dto.TsCount.Should().Be(0);
        dto.AdminApprovalId.Should().BeNull();
        dto.WorkCode.Should().BeNull();
    }
}

/// <summary>workCode is filled from the work master on the list and the detail read (V-04 has no work_code).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class DemandLineTable3WorkCodeTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"t3_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task T3_workCode_is_filled_on_list_and_detail()
    {
        // Give work A a code; every T-23 companion is set to satisfy the pairing CHECKs (triggers off: scaffolding).
        await using (var conn = new NpgsqlConnection(_w.ConnectionString))
        {
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SET session_replication_role = replica; " +
                "UPDATE budget.work SET work_code = '740-2526-009', work_code_origin = 'GENERATED', " +
                $"work_code_occasion = 'ALLOC_RBE', work_code_fin_year = 2026, work_code_at = now() WHERE id = {_w.WorkAId}; " +
                "SET session_replication_role = origin;", conn);
            await cmd.ExecuteNonQueryAsync();
        }
        Response added;
        await using (var ctx = _w.NewContext())
            added = await new AddDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                    TestServices.AllowAll(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                    new ConfigurationBuilder().Build(), new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
                .Handle(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 1 }), default);
        added.IsSucced.Should().BeTrue(added.Message);
        var lineId = ((Response<SaveDemandLineResponseDto>)added).Data!.DemandLineId;

        await using var c = _w.NewContext();
        var list = (await new GetDemandLinesByProposalHandler(c, _w.DivisionA, TestServices.OfficeScope(c))
            .Handle(new GetDemandLinesByProposalQuery(_w.WorksProposalAId, null, false, 1, 50), default)).Data!;
        list.Lines.Should().ContainSingle(l => l.DemandLineId == lineId).Which.WorkCode.Should().Be("740-2526-009");
        var detail = (await new GetDemandLineByIdHandler(c, _w.DivisionA, TestServices.OfficeScope(c))
            .Handle(new GetDemandLineByIdQuery(_w.WorksProposalAId, lineId), default)).Data!;
        detail.WorkCode.Should().Be("740-2526-009");
        detail.NameOfWork.Should().Be(detail.WorkDesc, "no A&A: the master name");
        detail.IsNameLocked.Should().BeFalse();
    }
}
