using BBMB.BudgetService.Infrastructure.DependencyInjection;
using BBMB.BudgetService.Shared.Endpoints.EndpointExtensions;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Services.EmployeeChargeServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>One mapped route: pattern, HTTP methods and the authorization metadata attached to it.</summary>
public sealed record CatalogEntry(
    string Pattern,
    IReadOnlyList<string> Methods,
    bool RequiresAuthorization,
    IReadOnlyList<string> Policies);

/// <summary>
/// Plan decision PD-2: proves the route table without Microsoft.AspNetCore.Mvc.Testing.
/// Builds a WebApplication with the services the endpoint delegates bind from DI, calls the
/// production <c>app.MapEndpoints()</c> and reads the resulting <see cref="EndpointDataSource"/>.
/// No request is served and no database is opened (the connection string is never used).
/// </summary>
public static class EndpointCatalog
{
    private static readonly Lazy<IReadOnlyList<CatalogEntry>> Entries = new(Build);

    public static IReadOnlyList<CatalogEntry> All => Entries.Value;

    public static CatalogEntry? Find(string method, string pattern) =>
        All.FirstOrDefault(e =>
            string.Equals(e.Pattern, pattern, StringComparison.OrdinalIgnoreCase) &&
            e.Methods.Contains(method, StringComparer.OrdinalIgnoreCase));

    private static IReadOnlyList<CatalogEntry> Build()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Test" });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=127.0.0.1;Database=catalog_unused",
        });

        // The DI registrations endpoint delegates bind from (mirrors Program.cs).
        builder.Services.AddDatabaseContexts(builder.Configuration);
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.Services.AddScoped<IFileStorageService, FileStorageService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IChargeResolver, ChargeResolver>();
        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.MapEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(e => new CatalogEntry(
                "/" + (e.RoutePattern.RawText ?? string.Empty).TrimStart('/'),
                e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.ToList() ?? [],
                e.Metadata.GetOrderedMetadata<IAuthorizeData>().Count > 0 &&
                e.Metadata.GetMetadata<IAllowAnonymous>() is null,
                e.Metadata.GetOrderedMetadata<IAuthorizeData>()
                    .Select(a => a.Policy)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Select(p => p!)
                    .ToList()))
            .ToList();
    }
}
