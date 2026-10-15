using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetById;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Rules;
using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Validators;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.AdminApprovals;
/// <summary>Design §8.4 revisions row (F-04; BUD-063, BUD-065, BUD-269; DB-06).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class ReviseAdminApprovalTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private long _approvalId;
    private long _lineId;
    private long _attachmentId;
    public async Task InitializeAsync()
    {
        _w = await SpineWorld.CreateAsync(fixture, $"aar_{Guid.NewGuid():N}"[..20]);
        _attachmentId = await CreateAdminApprovalTests.EvidenceAsync(_w, _w.DivisionA);
        var created = await CreateAdminApprovalTests.CreateAsync(_w,
            CreateAdminApprovalTests.Dto(_w, _attachmentId, workId: _w.WorkAId, amount: 1000));
        created.IsSucced.Should().BeTrue(created.Message);
        _approvalId = created.Data!.AdminApprovalId;
        _lineId = created.Data.DemandLineId!.Value;
    }
    public Task DisposeAsync() => Task.CompletedTask;

    private ReviseAdminApprovalDto Dto(decimal amount, long? accountCodeId = null, string reference = "AA/2026/1R") => new()
    {
        NameOfWork = "Lining of channel (revised)", AaReferenceNo = reference,
        DateOfApproval = RupeeRuleExtensions.TodayIst(), AccountCodeId = accountCodeId ?? _w.AccountCodeId,
        AmountSanctioned = amount, AaCopyAttachmentId = _attachmentId, CompetentAuthority = "SE",
    };

    private async Task<Response<int>> ReviseAsync(ReviseAdminApprovalDto dto, FakeBudgetActorSource? actor = null)
    {
        actor ??= _w.DivisionA;
        await using var ctx = _w.NewContext();
        return await new ReviseAdminApprovalHandler(ctx, actor, new BudgetActionScope(ctx, actor), TestServices.OfficeScope(ctx),
                new AdminApprovalRules(ctx, new DelegationLimitReader(ctx)), new WorkRules(ctx))
            .Handle(new ReviseAdminApprovalCommand(_approvalId, dto), default);
    }

    private async Task SetLineFiguresAsync(decimal rbe, decimal obe)
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "DEMAND_LINE_UPDATE");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            $"UPDATE budget.demand_line SET rbe_amount = {rbe}, obe_next_amount = {obe} WHERE demand_line_id = {_lineId}", c, tx);
        await cmd.ExecuteNonQueryAsync();
        await tx.CommitAsync();
    }

    private async Task<List<CorrectionFlag>> FlagsAsync()
    {
        await using var ctx = _w.NewContext();
        return await ctx.CorrectionFlags.AsNoTracking().Where(f => f.WorkId == _w.WorkAId).ToListAsync();
    }

    [Fact]
    public async Task BUD063_revision_writes_version_2_and_keeps_version_1()
    {
        var r = await ReviseAsync(Dto(1500));
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data.Should().Be(2);
        await using var ctx = _w.NewContext();
        var got = await new GetAdminApprovalByIdHandler(ctx, _w.DivisionA, TestServices.OfficeScope(ctx))
            .Handle(new GetAdminApprovalByIdQuery(_approvalId), default);
        got.Data!.CurrentVersionNo.Should().Be(2);
        got.Data.Versions.Select(v => (v.VersionNo, v.AmountSanctioned)).Should().Equal(((short)1, 1000m), ((short)2, 1500m));
        (await FlagsAsync()).Should().BeEmpty("an upward revision raises no flag");
    }

    [Fact]
    public async Task BUD065_reduced_amount_below_the_demand_raises_AA_REDUCED_and_does_not_alter_figures()
    {
        await SetLineFiguresAsync(600, 500);
        (await ReviseAsync(Dto(1000))).IsSucced.Should().BeTrue("1000 is below 1100 demanded");
        var flag = (await FlagsAsync()).Should().ContainSingle().Which;
        flag.Reason.Should().Be(CorrectionFlagReason.AaReduced);
        flag.FigureAllocated.Should().Be(1100);
        flag.FigureSupported.Should().Be(1000);
        await using var ctx = _w.NewContext();
        var line = await ctx.DemandLines.AsNoTracking().SingleAsync(l => l.DemandLineId == _lineId);
        (line.RbeAmount, line.ObeNextAmount).Should().Be((600m, 500m));
    }

    [Fact]
    public async Task BUD065_amount_equal_to_the_demand_raises_no_flag()
    {
        await SetLineFiguresAsync(600, 500);
        (await ReviseAsync(Dto(1100))).IsSucced.Should().BeTrue();
        (await FlagsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task BUD269_head_change_with_a_demand_line_raises_AA_HEAD_CHANGED()
    {
        var seg = await TestMasters.InsertSegmentsAsync(_w.ConnectionString);
        long otherCode;
        var (c, tx) = await _w.BeginAsync(_w.ItCell, "TEST_SETUP");
        await using (c)
        {
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
                "detail_acc_code, object_acc_code, head_desc, party_emp_required, origin, modification_date, user_code) " +
                $"VALUES ('{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', '{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', " +
                "'Other head', 'N', 'LEGACY', now(), 'T') RETURNING account_code_id", c, tx);
            otherCode = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            await tx.CommitAsync();
        }
        (await ReviseAsync(Dto(1500, otherCode))).IsSucced.Should().BeTrue();
        (await FlagsAsync()).Should().ContainSingle().Which.Reason.Should().Be(CorrectionFlagReason.AaHeadChanged);
    }

    [Fact]
    public async Task Revision_by_a_higher_office_is_M20_another_Division_NOT_FOUND_and_reference_of_another_approval_M19()
    {
        (await ReviseAsync(Dto(1500), _w.Circle)).MessageId.Should().Be(BudgetMessages.M20);
        (await ReviseAsync(Dto(1500), _w.DivisionB)).MessageId.Should().Be(BudgetMessages.NotFound);
        var att = await CreateAdminApprovalTests.EvidenceAsync(_w, _w.DivisionA);
        (await CreateAdminApprovalTests.CreateAsync(_w, CreateAdminApprovalTests.Dto(_w, att, reference: "AA/OTHER"))).IsSucced.Should().BeTrue();
        (await ReviseAsync(Dto(1500, reference: "AA/OTHER"))).MessageId.Should().Be(BudgetMessages.M19);
        (await ReviseAsync(Dto(1500, reference: "AA/2026/1"))).IsSucced.Should().BeTrue("its own earlier reference may repeat (DB-07)");
    }

    [Fact]
    public void No_PUT_or_DELETE_route_exists_for_approvals()
    {
        EndpointCatalog.All.Where(e => e.Pattern.StartsWith("/api/admin-approvals", StringComparison.Ordinal))
            .SelectMany(e => e.Methods).Should().OnlyContain(m => m == "GET" || m == "POST");
        EndpointCatalog.Find("POST", "/api/admin-approvals/{id:long}/revisions")!.Policies.Should().Equal("BudgetRight:F03.Enter");
    }
}
