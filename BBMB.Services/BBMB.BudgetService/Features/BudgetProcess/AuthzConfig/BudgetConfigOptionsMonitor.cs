using BBMB.BudgetService.Authorization.Requirements;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>
    /// Adapts the DB-backed <see cref="BudgetConfigSnapshot"/> onto the legacy
    /// <see cref="IOptionsMonitor{BudgetRightsOptions}"/> (design §2 "Adapter to the existing options type").
    /// <c>BudgetRightHandler</c> and <c>BudgetOfficeScope</c> keep reading <c>options.CurrentValue</c> and are
    /// not edited, so the policies and their tests stay green while the source becomes the database.
    /// <para>
    /// Registered as a singleton (an <see cref="IOptionsMonitor{T}"/> must be). <c>CurrentValue</c> is
    /// synchronous; it projects the singleton cache's snapshot when one is present (the common case within the
    /// TTL), and otherwise opens a short-lived scope to load through <see cref="IBudgetConfigProvider"/> — the
    /// provider then stores the snapshot in the shared cache, so subsequent reads are lock-free.
    /// </para>
    /// </summary>
    public sealed class BudgetConfigOptionsMonitor(
        IServiceScopeFactory scopeFactory,
        BudgetConfigCache cache) : IOptionsMonitor<BudgetRightsOptions>
    {
        public BudgetRightsOptions CurrentValue => Project(CurrentSnapshot());

        public BudgetRightsOptions Get(string? name) => CurrentValue;

        /// <summary>No change notifications are raised; readers poll <c>CurrentValue</c> within the cache TTL.</summary>
        public IDisposable? OnChange(Action<BudgetRightsOptions, string?> listener) => NullDisposable.Instance;

        private BudgetConfigSnapshot CurrentSnapshot()
        {
            if (cache.TryGetFresh(DateTime.UtcNow, out var fresh, out _))
                return fresh;

            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider.GetRequiredService<IBudgetConfigProvider>();
            // The provider refreshes and stores the snapshot in the shared singleton cache.
            return provider.GetAsync().GetAwaiter().GetResult();
        }

        private static BudgetRightsOptions Project(BudgetConfigSnapshot s) => new()
        {
            ProjectName = s.SsoProjectName,
            AdminRoles = s.AdminRoles,
            Rights = new Dictionary<string, string[]>(s.Rights, StringComparer.Ordinal),
            RoleOfficeLevels = new Dictionary<string, string>(s.RoleOfficeLevels, StringComparer.OrdinalIgnoreCase),
            QueryFieldRoles = new Dictionary<string, string[]>(s.QueryFieldRoles, StringComparer.Ordinal),
        };

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
