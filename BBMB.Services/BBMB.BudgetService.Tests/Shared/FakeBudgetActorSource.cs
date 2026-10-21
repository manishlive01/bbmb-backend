using System.Net;
using BBMB.BudgetService.Shared.Services.BudgetActors;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>Fixed actor for handler tests (design §14). Client IP 192.0.2.1 (TEST-NET-1).</summary>
public sealed class FakeBudgetActorSource(BudgetActor? actor, IReadOnlyList<Guid>? ambiguousChargeIds = null)
    : IBudgetActorSource
{
    public static readonly IPAddress TestIp = IPAddress.Parse("192.0.2.1");

    public BudgetActor? Actor { get; set; } = actor;

    public Task<BudgetActor?> GetAsync(CancellationToken ct = default) => Task.FromResult(Actor);

    public Task<BudgetActorResolution> ResolveAsync(CancellationToken ct = default) =>
        Task.FromResult(Actor is null
            ? new BudgetActorResolution(null, ambiguousChargeIds ?? [], ambiguousChargeIds is null ? "NO_ACTOR" : "AMBIGUOUS_CHARGE")
            : new BudgetActorResolution(Actor, [], null));

    public static FakeBudgetActorSource None() => new(null);
}
