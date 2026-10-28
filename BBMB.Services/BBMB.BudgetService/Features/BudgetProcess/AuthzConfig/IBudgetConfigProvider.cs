namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>
    /// Reads the current <see cref="BudgetConfigSnapshot"/> from the seven authz-config tables, through a
    /// short-TTL cache with a version stamp (design §2). Scoped; the cache behind it is a singleton, so a
    /// read is a cheap dictionary lookup within the TTL window.
    /// </summary>
    public interface IBudgetConfigProvider
    {
        /// <summary>The current snapshot. Returns <see cref="BudgetConfigSnapshot.Empty"/> when no active settings row exists.</summary>
        Task<BudgetConfigSnapshot> GetAsync(CancellationToken ct = default);

        /// <summary>Drops the cached snapshot so the next read rebuilds it (called by the config write endpoints).</summary>
        void Invalidate();
    }
}
