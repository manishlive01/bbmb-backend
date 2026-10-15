namespace BBMB.BudgetService.Shared.Services.Attachments
{
    /// <summary>Outcome of a malware scan (design §4.8, BUD-018).</summary>
    public enum ScanVerdict
    {
        /// <summary>Scanned and clean: the file may be stored.</summary>
        Clean,
        /// <summary>Malware found: 409 M-24 "failed the malware scan" (design §10).</summary>
        Infected,
        /// <summary>Scanner not configured or unreachable: 503 M-24 "scanning unavailable" (design §10).</summary>
        Unavailable,
    }

    /// <param name="Verdict">The outcome.</param>
    /// <param name="Engine">Name and signature version of the scanner, held in T-22 <c>scan_engine</c> (≤ 100).</param>
    /// <param name="Detail">The signature found, or the reason the scanner was unavailable. For the log only.</param>
    public sealed record ScanResult(ScanVerdict Verdict, string Engine, string? Detail = null);

    /// <summary>
    /// Malware scanner boundary (design §4.8, BUD-018). There is no scanner in the stack; providing clamd at
    /// the State Data Centre is OI-12. Outside Development an unconfigured scanner refuses every upload.
    /// </summary>
    public interface IAttachmentScanner
    {
        Task<ScanResult> ScanAsync(Stream content, CancellationToken ct);
    }
}
