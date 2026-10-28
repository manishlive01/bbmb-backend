namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>
    /// Singleton holding one cached <see cref="BudgetConfigSnapshot"/> with its version stamp and load time
    /// (design §2). The TTL is a const, not an appsettings key, so no configuration is read to read the
    /// configuration. Thread-safe: reads take a snapshot of the current entry, the provider swaps a new entry
    /// atomically, and <see cref="Invalidate"/> clears it so the next read fails the freshness check and rebuilds.
    /// </summary>
    public sealed class BudgetConfigCache
    {
        /// <summary>Bounded staleness: a DB change is visible within this window at worst (design §2).</summary>
        public static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

        private sealed record Entry(BudgetConfigSnapshot Snapshot, long Version, DateTime LoadedUtc);

        private volatile Entry? _entry;

        /// <summary>The cached entry if it is still within the TTL window, else null (the provider rebuilds).</summary>
        public bool TryGetFresh(DateTime nowUtc, out BudgetConfigSnapshot snapshot, out long version)
        {
            var entry = _entry;
            if (entry is not null && nowUtc - entry.LoadedUtc <= Ttl)
            {
                snapshot = entry.Snapshot;
                version = entry.Version;
                return true;
            }
            snapshot = BudgetConfigSnapshot.Empty;
            version = 0;
            return false;
        }

        /// <summary>The cached version stamp regardless of freshness, or null when nothing is cached.</summary>
        public long? CachedVersion => _entry?.Version;

        /// <summary>The cached snapshot regardless of freshness, or null when nothing is cached.</summary>
        public BudgetConfigSnapshot? CachedSnapshot => _entry?.Snapshot;

        /// <summary>Stores a freshly loaded snapshot and its version stamp.</summary>
        public void Store(BudgetConfigSnapshot snapshot, long version, DateTime nowUtc) =>
            _entry = new Entry(snapshot, version, nowUtc);

        /// <summary>Extends the TTL window without rebuilding when the version stamp is unchanged.</summary>
        public void Touch(DateTime nowUtc)
        {
            var entry = _entry;
            if (entry is not null)
                _entry = entry with { LoadedUtc = nowUtc };
        }

        /// <summary>Drops the cached entry; the next read rebuilds (called after an authorized config write).</summary>
        public void Invalidate() => _entry = null;
    }
}
