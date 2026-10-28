using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetList;
using BBMB.BudgetService.Features.BudgetProcess.Allocations.GetList;
using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search;
using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetById;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory;
using BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Features.BudgetProcess.Reports;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Reads;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetEstimateCodes;
using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Reads;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
namespace BBMB.BudgetService.Tests.Shared.Endpoints;
/// <summary>
/// Budget reads moved from GET to POST so that no id or filter travels in the URL (security requirement).
/// Proves each POST replacement is mapped with the same BudgetRight policy as the GET it replaced (the old GETs
/// are asserted absent in <see cref="RemovedRoutesTests"/>), and that the body validators refuse non-positive ids.
/// No database: route table via <see cref="EndpointCatalog"/>, validators called directly.
/// </summary>
public sealed class PostReadRoutesTests
{
    public static TheoryData<string, string> PostReads => new()
    {
        { "/api/account-head-assignments/list", "BudgetRight:Budget.View" },
        { "/api/admin-approvals/list", "BudgetRight:Budget.View" },
        { "/api/budget-cycles/allocations/preview", "BudgetRight:F19.Allocate" },
        { "/api/budget-cycles/consolidation", "BudgetRight:Budget.View" },
        { "/api/budget-cycles/distributions", "BudgetRight:Budget.View" },
        { "/api/budget-proposals/detail", "BudgetRight:Budget.View" },
        { "/api/budget-proposals/amr-predecessor-options", "BudgetRight:Budget.View" },
        { "/api/budget-proposals/assignable-heads", "BudgetRight:Budget.View" },
        { "/api/budget-proposals/demand-lines", "BudgetRight:Budget.View" },
        { "/api/budget-proposals/demand-lines/figure-history", "BudgetRight:Budget.View" },
        { "/api/budget-proposals/movements", "BudgetRight:Budget.View" },
        { "/api/budget-proposals/observations", "BudgetRight:Budget.View" },
        { "/api/correction-flags/list", "BudgetRight:Budget.View" },
        { "/api/reappropriations/detail", "BudgetRight:Budget.View" },
        { "/api/technical-sanctions/list", "BudgetRight:Budget.View" },
        { "/api/unforeseen-requests/detail", "BudgetRight:Budget.View" },
        // Follow-up: three POST reads that had no GET before (items #9, #15, #18).
        { "/api/budget-proposals/demand-lines/detail", "BudgetRight:Budget.View" },
        { "/api/reappropriations/sources/list", "BudgetRight:Budget.View" },
        { "/api/work-codes/estimate-codes", "BudgetRight:Budget.View" },
        // Pending-gap reads (BE-GAP-01, -03, -04, -05): new POST reads, plus the audit-trail search moved off GET.
        { "/api/budget-cycles/allocations/list", "BudgetRight:F19.Allocate" },
        { "/api/unforeseen-authorisations/detail", "BudgetRight:Budget.View" },
        { "/api/unforeseen-provisions/list", "BudgetRight:Budget.View" },
        { "/api/audit-trail/list", "BudgetRight:Budget.View" },
    };
    [Theory]
    [MemberData(nameof(PostReads))]
    public void Post_read_route_is_mapped_with_the_unchanged_policy(string pattern, string policy)
    {
        var entry = EndpointCatalog.Find("POST", pattern);
        entry.Should().NotBeNull(pattern);
        entry!.RequiresAuthorization.Should().BeTrue(pattern);
        entry.Policies.Should().Equal(policy);
        EndpointCatalog.Find("GET", pattern).Should().BeNull(pattern + ": no GET alias");
    }
    [Fact]
    public void Proposal_id_must_be_positive()
    {
        var v = new GetProposalByIdQueryValidator();
        var bad = v.Validate(new GetProposalByIdQuery(0));
        bad.IsValid.Should().BeFalse();
        bad.Errors.Should().ContainSingle(e => e.PropertyName == "proposalId");
        v.Validate(new GetProposalByIdQuery(12)).IsValid.Should().BeTrue();
    }
    [Fact]
    public void Figure_history_line_id_must_be_positive_and_is_named_lineId()
    {
        var bad = new GetFigureHistoryQueryValidator().Validate(new GetFigureHistoryQuery(12, 0));
        bad.IsValid.Should().BeFalse();
        bad.Errors.Should().ContainSingle(e => e.PropertyName == "lineId");
    }
    [Fact]
    public void Optional_filters_may_be_omitted_but_not_non_positive()
    {
        new GetCorrectionFlagsQueryValidator().Validate(new GetCorrectionFlagsQuery(null, true)).IsValid.Should().BeTrue();
        new GetCorrectionFlagsQueryValidator().Validate(new GetCorrectionFlagsQuery(0, true)).IsValid.Should().BeFalse();
        new GetHeadAssignmentsQueryValidator().Validate(new GetHeadAssignmentsQuery(null)).IsValid.Should().BeTrue();
        new GetHeadAssignmentsQueryValidator().Validate(new GetHeadAssignmentsQuery(-1)).IsValid.Should().BeFalse();
    }
    [Fact]
    public void Distributions_require_cycle_category_and_ce_office()
    {
        var v = new GetDistributionsQueryValidator();
        v.Validate(new GetDistributionsQuery(1, "", 301)).IsValid.Should().BeFalse();
        v.Validate(new GetDistributionsQuery(1, "DNA", 0)).IsValid.Should().BeFalse();
        v.Validate(new GetDistributionsQuery(1, "DNA", 301)).IsValid.Should().BeTrue();
    }
    [Fact]
    public void Detail_ids_must_be_positive()
    {
        new GetReappropriationByIdQueryValidator().Validate(new GetReappropriationByIdQuery(0))
            .Errors.Should().ContainSingle(e => e.PropertyName == "reappropriationId");
        new GetUnforeseenByIdQueryValidator().Validate(new GetUnforeseenByIdQuery(0))
            .Errors.Should().ContainSingle(e => e.PropertyName == "unforeseenRequestId");
    }
    [Fact]
    public void Add_source_write_is_untouched_by_the_sources_read()
    {
        EndpointCatalog.Find("POST", "/api/reappropriations/{id:long}/sources")!.Policies
            .Should().Equal("BudgetRight:F21.Enter");
    }
    [Fact]
    public void Demand_line_detail_needs_positive_proposal_and_line_ids()
    {
        var v = new GetDemandLineByIdQueryValidator();
        v.Validate(new GetDemandLineByIdQuery(12, 0)).Errors.Should().ContainSingle(e => e.PropertyName == "lineId");
        v.Validate(new GetDemandLineByIdQuery(0, 34)).Errors.Should().ContainSingle(e => e.PropertyName == "proposalId");
        v.Validate(new GetDemandLineByIdQuery(12, 34)).IsValid.Should().BeTrue();
    }
    [Fact]
    public void Reappropriation_sources_id_must_be_positive()
    {
        new GetReappropriationSourcesQueryValidator().Validate(new GetReappropriationSourcesQuery(0))
            .Errors.Should().ContainSingle(e => e.PropertyName == "reappropriationId");
        new GetReappropriationSourcesQueryValidator().Validate(new GetReappropriationSourcesQuery(5)).IsValid.Should().BeTrue();
    }
    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("1234567890123", false)] // 13 > Work.WorkCode max length 12
    [InlineData("101-2526-001", true)]   // generated form, exactly 12
    [InlineData("1234567", true)]        // legacy seven-digit form
    public void Work_code_must_be_present_and_within_the_column_length(string workCode, bool valid)
    {
        var result = new GetEstimateCodesByWorkCodeQueryValidator().Validate(new GetEstimateCodesByWorkCodeQuery(workCode));
        result.IsValid.Should().Be(valid);
        if (!valid) result.Errors.Should().ContainSingle(e => e.PropertyName == "workCode");
    }
    [Fact]
    public void Pending_gap_read_ids_must_be_positive_and_optional_filters_may_be_omitted()
    {
        new GetAllocationsQueryValidator().Validate(new GetAllocationsQuery(0))
            .Errors.Should().ContainSingle(e => e.PropertyName == "cycleId");
        new GetUnforeseenAuthorisationByIdQueryValidator().Validate(new GetUnforeseenAuthorisationByIdQuery(0))
            .Errors.Should().ContainSingle(e => e.PropertyName == "unforeseenAuthorisationId");
        new GetUnforeseenProvisionsQueryValidator().Validate(new GetUnforeseenProvisionsQuery(null, null)).IsValid.Should().BeTrue();
        new GetUnforeseenProvisionsQueryValidator().Validate(new GetUnforeseenProvisionsQuery(0, null)).IsValid.Should().BeFalse();
        new SearchAuditTrailQueryValidator().Validate(new SearchAuditTrailQuery(null, null, null, null, null, null, 1, 50)).IsValid.Should().BeTrue();
        new SearchAuditTrailQueryValidator().Validate(new SearchAuditTrailQuery(null, null, null, null, null, null, 1, 50, 0))
            .Errors.Should().ContainSingle(e => e.PropertyName == "cycleId");
    }
    [Fact]
    public void Query_fields_are_narrowed_by_configured_role()
    {
        var map = new Dictionary<string, string[]> { ["category"] = ["AFA_BUDGET", "DDO"], ["demandedRbe"] = ["AFA_BUDGET"] };
        QueryFieldCatalogue.ForRole("DDO", map).Select(f => f.Name).Should().Equal("category");
        QueryFieldCatalogue.ForRole("afa_budget", map).Select(f => f.Name).Should().Equal("category", "demandedRbe");
        QueryFieldCatalogue.ResolveForRole("demandedRbe", "DDO", map).Should().BeNull("DDO may not use demandedRbe");
        QueryFieldCatalogue.ResolveForRole("wing", "AFA_BUDGET", map).Should().BeNull("an unlisted field is refused");
        QueryFieldCatalogue.ForRole("DDO", new Dictionary<string, string[]>()).Should().HaveCount(QueryFieldCatalogue.Fields.Count,
            "an empty map keeps the full catalogue");
    }
}
