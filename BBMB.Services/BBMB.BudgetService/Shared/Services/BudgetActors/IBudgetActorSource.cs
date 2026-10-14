namespace BBMB.BudgetService.Shared.Services.BudgetActors
{
    /// <summary>Resolves the acting officer for the current request (design §4.3).</summary>
    public interface IBudgetActorSource
    {
        /// <summary>The actor, or null (the caller answers M-20).</summary>
        Task<BudgetActor?> GetAsync(CancellationToken ct = default);

        /// <summary>The actor with the reason for a null, and the charge ids when the charge is ambiguous.</summary>
        Task<BudgetActorResolution> ResolveAsync(CancellationToken ct = default);
    }
}
