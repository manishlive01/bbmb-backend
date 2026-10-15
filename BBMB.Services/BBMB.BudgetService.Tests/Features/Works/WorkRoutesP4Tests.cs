using BBMB.BudgetService.Features.Estimates.Create;
using BBMB.BudgetService.Features.Estimates.Create.Dtos;
using BBMB.BudgetService.Features.Works.Create;
using BBMB.BudgetService.Features.Works.Create.Dtos;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.Works;

/// <summary>
/// P4-03 (design §4.6 "API and code changes", §9.3 Works/Estimates P4, §13.2). The work-code reset re-keys
/// the Works and Estimates routes: the free code generator is gone (BUD-232), works are addressed by id,
/// a by-code lookup is added, estimate writes are refused (M-19), and a work is created with no code (DV-19).
/// </summary>
public sealed class WorkRoutesP4RouteTests
{
    [Fact]
    public void Works_routes_rekeyed_generate_code_gone_by_code_added()
    {
        // The free generator route is removed: a code is assigned only on allocation (BUD-232).
        EndpointCatalog.All.Should().NotContain(e => e.Pattern.Contains("/api/works/generate-code"),
            "the free work-code generator route is removed in P4 (BUD-232)");

        // A work is addressed by its stable id, and a by-code lookup is available.
        EndpointCatalog.Find("GET", "/api/works/{id:int}").Should().NotBeNull();
        EndpointCatalog.Find("PUT", "/api/works/{id:int}").Should().NotBeNull();
        EndpointCatalog.Find("GET", "/api/works/by-code/{workCode}").Should().NotBeNull();
    }

    [Fact]
    public void Estimate_routes_rekeyed_to_workId()
    {
        // Of the estimate routes, the DELETE (and the by-work GET) are IEndpoint classes that app.MapEndpoints()
        // discovers; they are re-keyed to {workId:int} and no {workCode} form remains on any mapped estimate route.
        EndpointCatalog.Find("DELETE", "/api/estimates/{workId:int}/{estimateCode}").Should().NotBeNull();
        EndpointCatalog.Find("GET", "/api/estimates/by-work/{workId:int}").Should().NotBeNull();
        EndpointCatalog.All.Should().NotContain(
            e => e.Pattern.StartsWith("/api/estimates") && e.Pattern.Contains("{workCode}"),
            "every mapped estimate route is re-keyed to work id after the reset");
        // GAP (raised, not fixed here): GetEstimateById and UpdateEstimate are static map-extension methods
        // (MapGetEstimateByIdEndpoint / MapUpdateEstimateEndpoint) that Program.cs never calls, so GET/PUT
        // /api/estimates/{workId:int}/{estimateCode} are not mapped at all. This is a pre-existing registration
        // gap, independent of P4; their query/command/handler are re-keyed to WorkId so they are correct once
        // wired. Wiring them adds reachable routes (a behaviour change) and is left for a reviewer decision.
    }

    [Fact]
    public void Budget_entries_by_work_route_rekeyed_to_workId()
    {
        EndpointCatalog.Find("GET", "/api/budgets/by-work/{workId:int}").Should().NotBeNull();
    }
}

/// <summary>Handler-level P4-03 behaviour on a migrated database (SpineWorld harness).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class WorkRoutesP4Tests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"wr_{Guid.NewGuid():N}"[..20]);

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task DV19_Work_created_through_the_work_master_carries_no_code()
    {
        var dto = new CreateWorkDto
        {
            WorkDesc = "Continuing work",
            WorkClass = "AA",
            DivisionId = _w.HierarchyA.DivisionId,
            AccountCodeId = _w.AccountCodeId,
            UserCode = "T",
            CECode = "1",
            CRCode = "1",
        };

        await using var ctx = _w.NewContext();
        var response = await new CreateWorkHandler(ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), new WorkRules(ctx))
            .Handle(new CreateWorkCommand(dto), default);
        response.IsSucced.Should().BeTrue(response.Message);

        await using var read = _w.NewContext();
        var work = await read.Works.AsNoTracking()
            .Where(w => w.DivisionId == _w.HierarchyA.DivisionId && w.WorkDesc == "Continuing work")
            .OrderByDescending(w => w.Id).FirstAsync();
        work.WorkCode.Should().BeNull("a work is created code-less; a code is generated only on allocation (BUD-232)");
        work.EnteredVia.Should().Be(WorkEnteredVia.LegacyOpening);
    }

    [Fact]
    public async Task Estimate_Create_is_refused_M19_after_the_reset()
    {
        // The legacy estimate write route no longer creates estimates (design §4.6, §11.3): an estimate code
        // is recorded only by Financial Accounting against a technical sanction. The handler refuses with M-19.
        var response = await new CreateEstimateHandler().Handle(
            new CreateEstimateCommand(new CreateEstimateDto { WorkCode = 1, EstimateCode = "E1", EstimateAmt = 100m, FinYearEnd = 2026, DivisionCode = 1 }),
            default);
        response.IsSucced.Should().BeFalse();
        response.MessageId.Should().Be("M-19");
    }
}
