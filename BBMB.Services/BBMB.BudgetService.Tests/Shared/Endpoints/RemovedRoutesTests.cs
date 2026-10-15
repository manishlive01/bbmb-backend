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
    };

    [Theory]
    [MemberData(nameof(Removed))]
    public void D9_RemovedRoutesAreAbsent(string method, string pattern)
    {
        EndpointCatalog.Find(method, pattern).Should().BeNull();
    }

    [Theory]
    [InlineData("/api/head-of-accounts")]
    [InlineData("/api/works/{code}")]
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
