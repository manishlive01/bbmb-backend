using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Shared.Endpoints;

/// <summary>Design §3 D-9, §4.7, §13.2 "Removed in P1".</summary>
public sealed class RemovedRoutesTests
{
    public static TheoryData<string, string> Removed => new()
    {
        // Deletes (D-9; DDD §3.3 no DELETE; a delete leaves no audit record).
        { "DELETE", "/api/head-of-accounts" },
        { "DELETE", "/api/works/{code}" },
        { "DELETE", "/api/Delete-MajorHead" },
        { "DELETE", "/api/accounting-heads/sub-major/{code}" },
        { "DELETE", "/api/minor-heads/{code}" },
        { "DELETE", "/api/sub-minor-heads/{code}" },
        { "DELETE", "/api/detail-heads/{code}" },
        { "DELETE", "/api/object-classifications/{code}" },
        // Updates (BUD-047).
        { "PUT", "/api/head-of-accounts" },
        { "PUT", "/api/Update-MajorHead" },
        { "PUT", "/api/accounting-heads/sub-major/{code}" },
        { "PUT", "/api/minor-heads/{code}" },
        { "PUT", "/api/sub-minor-heads/{code}" },
        { "PUT", "/api/detail-heads/{code}" },
        { "PUT", "/api/object-classifications/{code}" },
        // Per-segment creation (P2-10): F-01 is the single creation route (BUD-045, design §4.7).
        { "POST", "/api/Create-MajorHead" },
        { "POST", "/api/accounting-heads/sub-major" },
        { "POST", "/api/minor-heads" },
        { "POST", "/api/sub-minor-heads" },
        { "POST", "/api/detail-heads" },
        { "POST", "/api/object-classifications" },
        // GET → POST (no ids in the URL): these reads now take their ids and filters in a JSON body;
        // the POST replacements and their unchanged policies are asserted in PostReadRoutesTests.
        { "GET", "/api/account-head-assignments" },
        { "GET", "/api/admin-approvals" },
        { "GET", "/api/budget-cycles/{cycleId:long}/allocations/preview" },
        { "GET", "/api/budget-cycles/{cycleId:long}/consolidation" },
        { "GET", "/api/budget-cycles/{cycleId:long}/distributions" },
        { "GET", "/api/budget-proposals/{id:long}" },
        { "GET", "/api/budget-proposals/{id:long}/amr-predecessor-options" },
        { "GET", "/api/budget-proposals/{id:long}/assignable-heads" },
        { "GET", "/api/budget-proposals/{id:long}/demand-lines" },
        { "GET", "/api/budget-proposals/{id:long}/demand-lines/{lineId:long}/figure-history" },
        { "GET", "/api/budget-proposals/{id:long}/movements" },
        { "GET", "/api/budget-proposals/{id:long}/observations" },
        { "GET", "/api/correction-flags" },
        { "GET", "/api/reappropriations/{id:long}" },
        { "GET", "/api/technical-sanctions" },
        { "GET", "/api/unforeseen-requests/{id:long}" },
        // Follow-up POST reads (#9, #15, #18): no GET with ids in the URL was ever added for them.
        { "GET", "/api/budget-proposals/{id:long}/demand-lines/{lineId:long}" },
        { "GET", "/api/reappropriations/{id:long}/sources" },
        { "GET", "/api/work-codes/{workCode}/estimate-codes" },
        // Audit-trail search: filters moved from the query string into POST /api/audit-trail/list.
        { "GET", "/api/audit-trail" },
    };

    [Theory]
    [MemberData(nameof(Removed))]
    public void D9_RemovedRoutesAreAbsent(string method, string pattern)
    {
        EndpointCatalog.Find(method, pattern).Should().BeNull();
    }

    [Theory]
    [InlineData("/api/head-of-accounts")]
    // P4-03 (design §4.6): the work read route is addressed by id (/api/works/{id:int}); the former
    // /api/works/{code} form is gone with the int work code.
    [InlineData("/api/works/{id:int}")]
    [InlineData("/api/Get-MajorHead-List")]
    [InlineData("/api/accounting-heads/sub-major/{code}")]
    [InlineData("/api/minor-heads/{code}")]
    [InlineData("/api/sub-minor-heads/{code}")]
    [InlineData("/api/detail-heads/{code}")]
    [InlineData("/api/object-classifications/{code}")]
    public void D9_GET_routes_of_the_same_areas_still_exist(string pattern)
    {
        EndpointCatalog.Find("GET", pattern).Should().NotBeNull();
    }
}
