using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetWorkCandidates;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Rules;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetWorkCandidates;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Features.BudgetProcess.AdminApprovals;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.DemandLines;
/// <summary>Existing-work pickers: demand grid (work-candidates) and F-03 existing-work mode; Add with a foreign work.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class WorkCandidatesTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"wc_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;
    private async Task<Response> AddAsync(int workId)
    {
        await using var ctx = _w.NewContext();
        return await new AddDemandLineHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                new ConfigurationBuilder().Build(), new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(_w.WorksProposalAId, new AddDemandLineDto { LineType = "WORK", WorkId = workId }), default);
    }
    private async Task<List<DemandWorkCandidateDto>> DemandCandidatesAsync(string tab, FakeBudgetActorSource? actor = null)
    {
        await using var ctx = _w.NewContext();
        var r = await new GetDemandWorkCandidatesHandler(ctx, actor ?? _w.DivisionA, TestServices.OfficeScope(ctx))
            .Handle(new GetDemandWorkCandidatesQuery(_w.WorksProposalAId, tab), default);
        r.IsSucced.Should().BeTrue(r.Message);
        return r.Data!;
    }
    private async Task<Response<List<AaWorkCandidateDto>>> AaCandidatesAsync(FakeBudgetActorSource actor)
    {
        await using var ctx = _w.NewContext();
        return await new GetAaWorkCandidatesHandler(ctx, actor, new AdminApprovalRules(ctx, new DelegationLimitReader(ctx)))
            .Handle(new GetAaWorkCandidatesQuery(), default);
    }
    /// <summary>An AM&amp;R twin of work A in Division A (same head).</summary>
    private async Task<int> AmrWorkAsync()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "WORKS_ENTER");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.work (work_code, division_id, work_desc, user_code, work_class, entered_via, pre_ddd, " +
            "major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, " +
            "original_budget, revised_budget, proposed_budget, remarks) " +
            "SELECT NULL, division_id, 'AMR twin', 'T', 'AMR', 'LEGACY_OPENING', false, major_acc_code, sub_major_acc_code, " +
            "minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, '', '', '', '' " +
            $"FROM budget.work WHERE id = {_w.WorkAId} RETURNING id", c, tx);
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        await tx.CommitAsync();
        return id;
    }
    [Fact]
    public async Task Demand_candidates_are_filtered_by_division_class_and_not_already_lined()
    {
        var amr = await AmrWorkAsync();
        // A&A with a new work: the work is created and lined into the S-01 demand (BUD-059), so it is no candidate.
        var created = await CreateAdminApprovalTests.CreateAsync(_w,
            CreateAdminApprovalTests.Dto(_w, await CreateAdminApprovalTests.EvidenceAsync(_w, _w.DivisionA)));
        created.IsSucced.Should().BeTrue(created.Message);
        created.Data!.DemandLineId.Should().NotBeNull();
        var aa = await DemandCandidatesAsync("AA");
        aa.Select(c => c.WorkId).Should().Equal(_w.WorkAId); // not B (other Division), not the AMR work, not the lined one
        aa[0].AccountCodeId.Should().Be(_w.AccountCodeId);
        aa[0].HeadDesc.Should().Be("Test head");
        aa[0].CompleteCode.Should().NotBeNullOrEmpty();
        aa[0].NameOfWork.Should().Be("Work 500001");
        aa[0].AdminApprovalId.Should().BeNull();
        aa[0].HasTechnicalSanction.Should().BeFalse();
        (await DemandCandidatesAsync("AMR")).Select(c => c.WorkId).Should().Equal(amr);
        (await AddAsync(_w.WorkAId)).IsSucced.Should().BeTrue();
        (await DemandCandidatesAsync("AA")).Should().BeEmpty("work A is lined now");
        new GetDemandWorkCandidatesQueryValidator().Validate(new GetDemandWorkCandidatesQuery(_w.WorksProposalAId, "HEAD"))
            .IsValid.Should().BeFalse("an unknown tab is a 400");
        await using var ctx = _w.NewContext();
        (await new GetDemandWorkCandidatesHandler(ctx, _w.DivisionB, TestServices.OfficeScope(ctx))
            .Handle(new GetDemandWorkCandidatesQuery(_w.WorksProposalAId, "AA"), default))
            .MessageId.Should().Be(BudgetMessages.NotFound, "Division B does not see Division A's proposal");
    }
    [Fact]
    public async Task Aa_candidates_exclude_works_with_an_approval_and_flag_the_demand()
    {
        var first = (await AaCandidatesAsync(_w.DivisionA)).Data!;
        first.Should().ContainSingle().Which.Should().Match<AaWorkCandidateDto>(c => c.WorkId == _w.WorkAId && c.DemandProposalId == null);
        (await AddAsync(_w.WorkAId)).IsSucced.Should().BeTrue();
        (await AaCandidatesAsync(_w.DivisionA)).Data!.Single().DemandProposalId.Should().Be(_w.WorksProposalAId);
        var created = await CreateAdminApprovalTests.CreateAsync(_w,
            CreateAdminApprovalTests.Dto(_w, await CreateAdminApprovalTests.EvidenceAsync(_w, _w.DivisionA), workId: _w.WorkAId));
        created.IsSucced.Should().BeTrue(created.Message);
        (await AaCandidatesAsync(_w.DivisionA)).Data!.Should().BeEmpty("work A has its approval");
        (await AaCandidatesAsync(_w.DivisionB)).Data!.Select(c => c.WorkId).Should().Equal(_w.WorkBId);
        var circle = await AaCandidatesAsync(_w.Circle);
        circle.IsSucced.Should().BeFalse();
        circle.MessageId.Should().Be(BudgetMessages.M20);
    }
    [Fact]
    public async Task Add_with_an_existing_work_of_another_division_is_refused()
    {
        var r = await AddAsync(_w.WorkBId);
        r.IsSucced.Should().BeFalse();
        ((Response<RuleFailuresDto>)r).Data!.Failures.Should().ContainSingle(f => f.Field == "workId");
        var again = await AddAsync(_w.WorkAId);
        again.IsSucced.Should().BeTrue();
        var dup = await AddAsync(_w.WorkAId);
        ((Response<RuleFailuresDto>)dup).Data!.Failures.Should().Contain(f => f.Field == "workId" && f.MessageId == BudgetMessages.M25);
    }
}
