using System.Text.Json;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Shared.Endpoints;

/// <summary>Design §3 D-1, §4.4 default rights, §9.3 (PD-2: route table read through EndpointCatalog).</summary>
public sealed class BudgetEndpointPolicyTests
{
    private static readonly string[] BudgetAreas =
    [
        "Budgets", "Works", "Estimates", "HeadOfAccounts", "AccountingHeads", "DetailHeads",
        "ObjectClassifications", "FinancialYears", "BulkExcelUploads",
    ];

    private static string? AreaOf(CatalogEntry e)
    {
        const string prefix = "BBMB.BudgetService.Features.";
        if (!e.Namespace.StartsWith(prefix, StringComparison.Ordinal)) return null;
        var area = e.Namespace[prefix.Length..].Split('.')[0];
        return BudgetAreas.Contains(area) ? area : null;
    }

    /// <summary>Design §9.3 as rules: the right key each existing budget-area route must carry.</summary>
    private static string ExpectedKey(CatalogEntry e, string area)
    {
        var slice = e.Namespace.Split('.').Last();
        var method = e.Methods.Single();
        if (area == "BulkExcelUploads") return "Budget.View"; // handler authorises per upload type
        if (area == "Works" && slice == "GenerateWorkCodes") return "Works.Enter";
        if (area == "HeadOfAccounts" && slice == "MarkNotForUse") return "F01.MarkNotForUse";
        if (area == "HeadOfAccounts" && slice == "SetPartyEmpFlag") return "F01.FlagPartyEmp";
        var read = method == "GET" || slice is "GetById" or "GetDropdown" or "GetList";
        if (read) return "Budget.View";
        return area switch
        {
            "Budgets" or "Estimates" => "Legacy.BudgetEntry",
            "Works" => "Works.Enter",
            "FinancialYears" => "Cycle.Manage",
            _ => "F01.Create",
        };
    }

    private static HashSet<string> DefaultRightKeys()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        return doc.RootElement.GetProperty("BudgetRights").GetProperty("Rights")
            .EnumerateObject().Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
    }

    [Fact]
    public void D1_EveryBudgetAreaEndpointCarriesABudgetRightPolicy()
    {
        var routes = EndpointCatalog.All.Where(e => AreaOf(e) is not null).ToList();
        routes.Should().HaveCountGreaterThan(30, "the nine areas are mapped");
        var keys = DefaultRightKeys();

        foreach (var e in routes)
        {
            var label = $"{string.Join(",", e.Methods)} {e.Pattern} ({e.EndpointType?.Name})";
            var rights = e.Policies.Where(p => p.StartsWith("BudgetRight:", StringComparison.Ordinal)).ToList();
            rights.Should().ContainSingle(label);
            e.Policies.Should().NotContain(p => p.StartsWith("Budget:", StringComparison.Ordinal), label + ": legacy role string removed");

            var key = rights[0]["BudgetRight:".Length..];
            key.Should().Be(ExpectedKey(e, AreaOf(e)!), label);
            keys.Should().Contain(key, label + ": key exists in the default BudgetRights");
        }
    }

    [Fact]
    public void S81_EveryBudgetProcessEndpointCarriesOneBudgetRightPolicyFromTheMatrix()
    {
        var routes = EndpointCatalog.All
            .Where(e => e.Namespace.StartsWith("BBMB.BudgetService.Features.BudgetProcess.", StringComparison.Ordinal)).ToList();
        routes.Should().HaveCountGreaterThan(25);
        var keys = DefaultRightKeys();
        foreach (var e in routes)
        {
            var label = $"{string.Join(",", e.Methods)} {e.Pattern} ({e.EndpointType?.Name})";
            e.RequiresAuthorization.Should().BeTrue(label);
            var rights = e.Policies.Where(p => p.StartsWith("BudgetRight:", StringComparison.Ordinal)).ToList();
            rights.Should().ContainSingle(label);
            keys.Should().Contain(rights[0]["BudgetRight:".Length..], label + ": key exists in the default BudgetRights");
        }
    }

    [Fact]
    public void D1_POST_api_budgets_requires_authorization()
    {
        var create = EndpointCatalog.Find("POST", "/api/budgets");
        create.Should().NotBeNull();
        create!.RequiresAuthorization.Should().BeTrue();
        create.Policies.Should().Equal("BudgetRight:Legacy.BudgetEntry");
    }
}
