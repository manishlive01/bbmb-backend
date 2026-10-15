using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.HeadOfAccounts.Create;
using BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos;
using BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse;
using BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse.Dtos;
using BBMB.BudgetService.Features.HeadOfAccounts.Rules;
using BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag;
using BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag.Dtos;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Features.BudgetProcess.AdminApprovals;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.HeadOfAccounts;
/// <summary>Design §8.4 head-of-accounts rows (F-01; VR-01, BUD-041 to BUD-046, BUD-267, BUD-268, BUD-308, BUD-322; D-9).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class AccountCodeTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"acc_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>The world's six existing segments, selected (no descriptions).</summary>
    private CreateHeadOfAccountDto Existing() => new()
    {
        MajorAccCode = _w.Segments.Major, SubMajorAccCode = _w.Segments.SubMajor, MinorAccCode = _w.Segments.Minor,
        SubMinorAccCode = _w.Segments.SubMinor, DetailAccCode = _w.Segments.Detail, ObjectAccCode = _w.Segments.Object,
        HeadDesc = "Head", EofficeApprovalRef = "EO/2026/7",
    };

    private async Task<Response> CreateAsync(CreateHeadOfAccountDto dto, FakeBudgetActorSource? actor = null)
    {
        actor ??= _w.ItCell;
        await using var ctx = _w.NewContext();
        return await new CreateHeadOfAccountHandler(ctx, actor, new BudgetActionScope(ctx, actor), new AccountCodeRules(ctx))
            .Handle(new CreateHeadOfAccountCommand(dto), default);
    }

    private async Task<Response> MarkAsync(long code)
    {
        await using var ctx = _w.NewContext();
        return await new MarkNotForUseHandler(ctx, _w.ItCell, new BudgetActionScope(ctx, _w.ItCell), new AccountCodeRules(ctx))
            .Handle(new MarkNotForUseCommand(code, new MarkNotForUseDto { EofficeRef = "EO/2026/9" }), default);
    }

    [Fact]
    public async Task VR01_duplicate_complete_code_is_M02()
    {
        (await CreateAsync(Existing())).MessageId.Should().Be(BudgetMessages.M02);
    }

    [Fact]
    public async Task BUD322_a_description_sent_for_an_existing_segment_is_M01_and_a_new_segment_without_one_is_M21()
    {
        var dto = Existing();
        dto.MajorAccDesc = "Re-entered nomenclature";
        dto.ObjectAccCode = "998";            // new object code, no description
        var r = (Response<RuleFailuresDto>)await CreateAsync(dto);
        r.Data!.Failures.Select(f => (f.Field, f.MessageId)).Should().BeEquivalentTo(
            [("majorAccDesc", BudgetMessages.M01), ("objectAccDesc", BudgetMessages.M21)]);
        await using var ctx = _w.NewContext();
        (await ctx.ObjectClassifications.AnyAsync(o => o.ObjectAccCode == "998")).Should().BeFalse("nothing is written");
    }

    [Fact]
    public async Task BUD045_new_segments_and_the_code_are_created_together_as_CREATED_and_selectable()
    {
        var dto = Existing();
        dto.ObjectAccCode = "997";
        dto.ObjectAccDesc = "New object";
        var r = await CreateAsync(dto);
        r.IsSucced.Should().BeTrue(r.Message);
        var data = ((Response<CreateAccountCodeResponseDto>)r).Data!;
        data.CompleteCode.Should().EndWith("-997");
        await using var ctx = _w.NewContext();
        var head = await ctx.HeadOfAccounts.AsNoTracking().SingleAsync(h => h.AccountCodeId == data.AccountCodeId);
        head.Origin.Should().Be("CREATED");
        head.EofficeApprovalRef.Should().Be("EO/2026/7");
        head.NotForUse.Should().BeFalse("offered for selection at once (BUD-046)");
        (await ctx.ObjectClassifications.SingleAsync(o => o.ObjectAccCode == "997")).ObjectAccDesc.Should().Be("New object");
    }

    [Fact]
    public void BUD044_missing_eoffice_reference_is_400_M03_and_segment_patterns_are_enforced()
    {
        var v = new CreateHeadOfAccountValidator();
        var dto = Existing();
        dto.EofficeApprovalRef = "";
        v.Validate(new CreateHeadOfAccountCommand(dto)).Errors.Should().Contain(e => e.ErrorCode == BudgetMessages.M03);
        var bad = Existing();
        bad.MajorAccCode = "12A4";
        v.Validate(new CreateHeadOfAccountCommand(bad)).IsValid.Should().BeFalse();
        v.Validate(new CreateHeadOfAccountCommand(Existing())).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task BUD308_marking_a_code_used_by_a_demand_line_is_M33_and_an_unused_code_is_marked_once()
    {
        var dto = Existing();
        dto.ObjectAccCode = "996";
        dto.ObjectAccDesc = "Spare object";
        var fresh = ((Response<CreateAccountCodeResponseDto>)await CreateAsync(dto)).Data!.AccountCodeId;
        var line = await AddWorkLineAsync(_w.AccountCodeId);
        line.IsSucced.Should().BeTrue(line.Message);
        (await MarkAsync(_w.AccountCodeId)).MessageId.Should().Be(BudgetMessages.M33);
        (await MarkAsync(fresh)).IsSucced.Should().BeTrue();
        (await MarkAsync(fresh)).MessageId.Should().Be(BudgetMessages.M19, "the mark is one-way and set once");
        await using var ctx = _w.NewContext();
        var head = await ctx.HeadOfAccounts.AsNoTracking().SingleAsync(h => h.AccountCodeId == fresh);
        (head.NotForUseRef, head.NotForUseBy).Should().Be(("EO/2026/9", _w.ItCell.Actor!.EmployeeId));
    }

    [Fact]
    public async Task BUD308_a_code_marked_not_for_use_is_M05_at_F07_Add_and_F03_Create()
    {
        var dto = Existing();
        dto.ObjectAccCode = "995";
        dto.ObjectAccDesc = "Wrong object";
        var wrong = ((Response<CreateAccountCodeResponseDto>)await CreateAsync(dto)).Data!.AccountCodeId;
        (await MarkAsync(wrong)).IsSucced.Should().BeTrue();

        var f07 = (Response<RuleFailuresDto>)await AddNewWorkLineAsync(wrong);
        f07.Data!.Failures.Should().ContainSingle(f => f.MessageId == BudgetMessages.M05);

        var att = await CreateAdminApprovalTests.EvidenceAsync(_w, _w.DivisionA);
        var aa = CreateAdminApprovalTests.Dto(_w, att);
        aa.AccountCodeId = wrong;
        (await CreateAdminApprovalTests.CreateAsync(_w, aa)).MessageId.Should().Be(BudgetMessages.M05);
    }

    [Fact]
    public async Task BUD268_party_flag_by_a_DDO_is_M20_and_by_the_IT_Cell_is_set()
    {
        async Task<Response> Set(FakeBudgetActorSource a, string v)
        {
            await using var ctx = _w.NewContext();
            return await new SetPartyEmpFlagHandler(ctx, a, new BudgetActionScope(ctx, a))
                .Handle(new SetPartyEmpFlagCommand(_w.AccountCodeId, new SetPartyEmpFlagDto { Value = v }), default);
        }
        (await Set(_w.DivisionA, "Y")).MessageId.Should().Be(BudgetMessages.M20, "403 through BudgetResults");
        (await Set(_w.ItCell, "Y")).IsSucced.Should().BeTrue();
        await using var read = _w.NewContext();
        (await read.HeadOfAccounts.AsNoTracking().SingleAsync(h => h.AccountCodeId == _w.AccountCodeId)).PartyEmpRequired.Should().Be("Y");
    }

    [Fact]
    public void D9_the_six_segment_Create_routes_are_absent_and_F01_is_the_creation_route()
    {
        foreach (var route in new[] { "/api/Create-MajorHead", "/api/accounting-heads/sub-major", "/api/minor-heads",
                     "/api/sub-minor-heads", "/api/detail-heads", "/api/object-classifications" })
            EndpointCatalog.Find("POST", route).Should().BeNull(route);
        EndpointCatalog.Find("POST", "/api/head-of-accounts")!.Policies.Should().Equal("BudgetRight:F01.Create");
        EndpointCatalog.Find("POST", "/api/head-of-accounts/{accountCodeId:long}/not-for-use")!.Policies.Should().Equal("BudgetRight:F01.MarkNotForUse");
        EndpointCatalog.Find("PUT", "/api/head-of-accounts/{accountCodeId:long}/party-emp-flag")!.Policies.Should().Equal("BudgetRight:F01.FlagPartyEmp");
    }

    private async Task<Response> AddWorkLineAsync(long code)
    {
        await using var ctx = _w.NewContext();
        return await new AddDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, new ConfigurationBuilder().Build(),
                new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, AccountCodeId = code }), default);
    }

    private async Task<Response> AddNewWorkLineAsync(long code)
    {
        await using var ctx = _w.NewContext();
        return await new AddDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, new ConfigurationBuilder().Build(),
                new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto
            {
                LineType = "WORK", NewWork = new() { NameOfWork = "New work", AccountCodeId = code, WorkClass = "AA" },
            }), default);
    }
}
