using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetById;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.Works.Entities;
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
/// <summary>Design §8.4 admin-approval rows (F-03; BUD-052 to BUD-059, BUD-063, BUD-067).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class CreateAdminApprovalTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"aac_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;

    internal static async Task<long> EvidenceAsync(SpineWorld w, FakeBudgetActorSource actor, string evidenceClass = "EVIDENCE")
    {
        var (c, tx) = await w.BeginAsync(actor, "ATTACHMENT_UPLOAD");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.attachment (storage_key, original_file_name, file_extension, content_type, evidence_class, " +
            "size_bytes, sha256_digest, scan_engine, scanned_at) VALUES (gen_random_uuid(), 'aa.pdf', 'pdf', 'application/pdf', " +
            $"'{evidenceClass}', 1024, repeat('a', 64), 'TEST', now()) RETURNING attachment_id", c, tx);
        var id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
        await tx.CommitAsync();
        return id;
    }

    internal static CreateAdminApprovalDto Dto(SpineWorld w, long attachmentId, int? workId = null, decimal amount = 100_000,
        string reference = "AA/2026/1", string authority = "EE", string? justification = null) => new()
    {
        WorkId = workId, NewWork = workId is null ? new NewAaWorkDto() : null, NameOfWork = "Lining of channel",
        AaReferenceNo = reference, DateOfApproval = RupeeRuleExtensions.TodayIst().AddDays(-1), AccountCodeId = w.AccountCodeId,
        AmountSanctioned = amount, AaCopyAttachmentId = attachmentId, CompetentAuthority = authority,
        DelegationJustification = justification,
    };

    internal static async Task<Response<CreateAdminApprovalResponseDto>> CreateAsync(SpineWorld w, CreateAdminApprovalDto dto, FakeBudgetActorSource? actor = null)
    {
        actor ??= w.DivisionA;
        await using var ctx = w.NewContext();
        return await new CreateAdminApprovalHandler(ctx, actor, new BudgetActionScope(ctx, actor),
                new AdminApprovalRules(ctx, new DelegationLimitReader(ctx)), new WorkRules(ctx))
            .Handle(new CreateAdminApprovalCommand(dto), default);
    }

    private async Task<int> ApprovalCountAsync()
    {
        await using var ctx = _w.NewContext();
        return await ctx.AdminApprovals.CountAsync();
    }

    private async Task LimitAsync(string level, decimal max)
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell, "DELEGATION_LIMIT_CREATE");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            $"INSERT INTO budget.delegation_limit (authority_level, max_amount, effective_from, source_ref) VALUES ('{level}', {max}, current_date - 30, 'R-06')", c, tx);
        await cmd.ExecuteNonQueryAsync();
        await tx.CommitAsync();
    }

    [Fact]
    public void BUD014_date_of_approval_tomorrow_IST_is_refused_by_the_validator()
    {
        var dto = Dto(_w, 1);
        dto.DateOfApproval = RupeeRuleExtensions.TodayIst().AddDays(1);
        new CreateAdminApprovalValidator().Validate(new CreateAdminApprovalCommand(dto)).IsValid.Should().BeFalse();
        dto.DateOfApproval = RupeeRuleExtensions.TodayIst();
        new CreateAdminApprovalValidator().Validate(new CreateAdminApprovalCommand(dto)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void BUD057_missing_copy_is_M04_and_paise_and_both_work_forms_are_refused()
    {
        var v = new CreateAdminApprovalValidator();
        var dto = Dto(_w, 1);
        dto.AaCopyAttachmentId = null;
        v.Validate(new CreateAdminApprovalCommand(dto)).Errors.Should().Contain(e => e.ErrorCode == BudgetMessages.M04);
        var paise = Dto(_w, 1, amount: 100.5m);
        v.Validate(new CreateAdminApprovalCommand(paise)).Errors.Should().Contain(e => e.ErrorCode == BudgetMessages.M22);
        var both = Dto(_w, 1, workId: 5);
        both.NewWork = new NewAaWorkDto();
        v.Validate(new CreateAdminApprovalCommand(both)).IsValid.Should().BeFalse();
        var amr = Dto(_w, 1);
        amr.NewWork = new NewAaWorkDto { WorkClass = "AMR" };
        v.Validate(new CreateAdminApprovalCommand(amr)).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task BUD058_account_code_marked_not_for_use_is_refused_M05()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell, "F01_MARK_NFU");
        await using (c)
        {
            await using var cmd = new NpgsqlCommand(
                "UPDATE budget.budget_head_master SET not_for_use = true, not_for_use_ref = 'EO/1', not_for_use_at = now(), " +
                $"not_for_use_by = '{_w.ItCell.Actor!.EmployeeId}' WHERE account_code_id = {_w.AccountCodeId}", c, tx);
            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        var att = await EvidenceAsync(_w, _w.DivisionA);
        (await CreateAsync(_w, Dto(_w, att))).MessageId.Should().Be(BudgetMessages.M05);
        (await ApprovalCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task BUD055_above_delegation_without_justification_is_M27_with_flag_and_nothing_saved_then_saved_with_justification()
    {
        await LimitAsync("EE", 50_000);
        var att = await EvidenceAsync(_w, _w.DivisionA);
        var refused = await CreateAsync(_w, Dto(_w, att, amount: 50_001));
        refused.MessageId.Should().Be(BudgetMessages.M27);
        refused.Data!.ExceedsDelegation.Should().BeTrue();
        (await ApprovalCountAsync()).Should().Be(0);
        await using (var ctx = _w.NewContext())
            (await ctx.Works.CountAsync(w => w.EnteredVia == WorkEnteredVia.AaEntry)).Should().Be(0, "nothing is saved, not even the work");

        var saved = await CreateAsync(_w, Dto(_w, att, amount: 50_001, justification: "Accorded by EE in an emergency"));
        saved.IsSucced.Should().BeTrue(saved.Message);
        saved.Data!.ExceedsDelegation.Should().BeTrue();
        await using var ctx2 = _w.NewContext();
        var v = await ctx2.AdminApprovalVersions.SingleAsync();
        v.ExceedsDelegation.Should().BeTrue();
        v.DelegationJustification.Should().Be("Accorded by EE in an emergency");
    }

    [Fact]
    public async Task BUD055_at_the_limit_is_not_flagged()
    {
        await LimitAsync("EE", 50_000);
        var att = await EvidenceAsync(_w, _w.DivisionA);
        var r = await CreateAsync(_w, Dto(_w, att, amount: 50_000));
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.ExceedsDelegation.Should().BeFalse();
    }

    [Fact]
    public async Task BUD059_new_work_approval_adds_a_WORK_line_with_null_figures_when_the_WORKS_demand_is_at_S01()
    {
        var att = await EvidenceAsync(_w, _w.DivisionA);
        var r = await CreateAsync(_w, Dto(_w, att));
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.SerialNo.Should().Be(1, "allotted by the module (BUD-053)");
        r.Data.DemandLineId.Should().NotBeNull();
        await using var ctx = _w.NewContext();
        var work = await ctx.Works.SingleAsync(w => w.Id == r.Data.WorkId);
        work.EnteredVia.Should().Be(WorkEnteredVia.AaEntry);
        work.WorkClass.Should().Be(WorkClass.Aa);
        work.DivisionId.Should().Be(_w.HierarchyA.DivisionId);
        var line = await ctx.DemandLines.SingleAsync(l => l.DemandLineId == r.Data.DemandLineId);
        line.ProposalId.Should().Be(_w.WorksProposalAId);
        line.WorkId.Should().Be(work.Id);
        line.RbeAmount.Should().BeNull();
        line.ObeNextAmount.Should().BeNull();
        line.AccountCodeId.Should().Be(_w.AccountCodeId);
    }

    [Fact]
    public async Task BUD059_with_no_current_cycle_the_approval_is_saved_and_no_line_is_added()
    {
        var w = await SpineWorld.CreateAsync(fixture, $"aan_{Guid.NewGuid():N}"[..20], withCycle: false);
        var att = await EvidenceAsync(w, w.DivisionA);
        var r = await CreateAsync(w, Dto(w, att));
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.DemandLineId.Should().BeNull();
        await using var ctx = w.NewContext();
        (await ctx.DemandLines.CountAsync()).Should().Be(0);
        (await ctx.AdminApprovalVersions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task BUD052_higher_office_is_M20_and_another_Divisions_DDO_does_not_see_the_work()
    {
        var circleAtt = await EvidenceAsync(_w, _w.Circle);
        (await CreateAsync(_w, Dto(_w, circleAtt, workId: _w.WorkAId), _w.Circle)).MessageId.Should().Be(BudgetMessages.M20);
        var bAtt = await EvidenceAsync(_w, _w.DivisionB);
        (await CreateAsync(_w, Dto(_w, bAtt, workId: _w.WorkAId), _w.DivisionB)).MessageId
            .Should().Be(BudgetMessages.NotFound, "design §4.9: a record outside the actor's offices is NOT_FOUND, never 403");
        (await CreateAsync(_w, Dto(_w, bAtt, workId: _w.WorkBId), _w.DivisionB)).IsSucced.Should().BeTrue("Division B's own work");
    }

    [Fact]
    public async Task Existing_work_second_approval_and_a_reused_reference_are_M19()
    {
        var att = await EvidenceAsync(_w, _w.DivisionA);
        var first = await CreateAsync(_w, Dto(_w, att, workId: _w.WorkAId));
        first.IsSucced.Should().BeTrue(first.Message);
        first.Data!.DemandLineId.Should().NotBeNull();
        (await CreateAsync(_w, Dto(_w, att, workId: _w.WorkAId, reference: "AA/2026/2"))).MessageId.Should().Be(BudgetMessages.M19, "one approval per work");
        (await CreateAsync(_w, Dto(_w, att))).MessageId.Should().Be(BudgetMessages.M19, "DB-07: the reference is used in the Division");
        var second = await CreateAsync(_w, Dto(_w, att, reference: "AA/2026/2"));
        second.Data!.SerialNo.Should().Be(2);
    }

    [Fact]
    public async Task VR10_supporting_copy_or_another_officers_upload_is_M04()
    {
        var supporting = await EvidenceAsync(_w, _w.DivisionA, "SUPPORTING");
        (await CreateAsync(_w, Dto(_w, supporting))).MessageId.Should().Be(BudgetMessages.M04);
        var other = await TestActor.CreateAsync(fixture, role: "DDO", officeId: _w.DivisionAOfficeId, officeLevel: "DIVISION",
            name: "Other DDO", connectionString: _w.ConnectionString);
        var othersUpload = await EvidenceAsync(_w, other);
        (await CreateAsync(_w, Dto(_w, othersUpload))).MessageId.Should().Be(BudgetMessages.M04);
        (await ApprovalCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Pre_ddd_work_is_refused_M21()
    {
        // A pre-DDD work exists only from before BudgetP1ProposalSpine; it is seeded here with triggers suspended
        // (session_replication_role = replica, test data only), as the paise-guard test seeds legacy rows.
        var seg = _w.Segments;
        int workId;
        await using (var c = new NpgsqlConnection(_w.ConnectionString))
        {
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SET session_replication_role = replica; " +
                "INSERT INTO budget.work (work_code, division_id, work_desc, user_code, pre_ddd, major_acc_code, sub_major_acc_code, " +
                "minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, original_budget, revised_budget, proposed_budget, remarks) " +
                $"VALUES (599999, {_w.HierarchyA.DivisionId}, 'Old work', 'T', true, '{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', " +
                $"'{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', '', '', '', '') RETURNING id", c);
            workId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        var att = await EvidenceAsync(_w, _w.DivisionA);
        (await CreateAsync(_w, Dto(_w, att, workId: workId))).MessageId.Should().Be(BudgetMessages.M21);
    }

    [Fact]
    public async Task BUD067_GetById_is_scoped_and_returns_every_version()
    {
        var att = await EvidenceAsync(_w, _w.DivisionA);
        var id = (await CreateAsync(_w, Dto(_w, att))).Data!.AdminApprovalId;
        async Task<Response<BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Dtos.AdminApprovalDto>> Get(FakeBudgetActorSource a)
        {
            await using var ctx = _w.NewContext();
            return await new GetAdminApprovalByIdHandler(ctx, a, TestServices.OfficeScope(ctx)).Handle(new GetAdminApprovalByIdQuery(id), default);
        }
        var circle = await Get(_w.Circle);
        circle.IsSucced.Should().BeTrue();
        circle.Data!.Versions.Should().ContainSingle().Which.AaCopyUrl.Should().Be($"/api/budget-attachments/{att}");
        (await Get(_w.DivisionB)).MessageId.Should().Be(BudgetMessages.NotFound);
    }

    [Fact]
    public void Routes_carry_F03_Enter_and_Budget_View()
    {
        EndpointCatalog.Find("POST", "/api/admin-approvals")!.Policies.Should().Equal("BudgetRight:F03.Enter");
        EndpointCatalog.Find("GET", "/api/admin-approvals")!.Policies.Should().Equal("BudgetRight:Budget.View");
        EndpointCatalog.Find("GET", "/api/admin-approvals/{id:long}")!.Policies.Should().Equal("BudgetRight:Budget.View");
    }
}
