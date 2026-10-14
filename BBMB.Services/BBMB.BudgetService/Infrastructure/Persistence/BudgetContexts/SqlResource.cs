using System.Reflection;

namespace BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts
{
    /// <summary>
    /// Reads the hand-written SQL that migrations execute (design §2.1, §11.1 rule 2). The files live in
    /// <c>Infrastructure/Persistence/BudgetContexts/Sql/</c> and are embedded in the assembly, so a
    /// migration bundle carries exactly the text that was reviewed.
    /// </summary>
    public static class SqlResource
    {
        private const string Prefix = "BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts.Sql.";

        /// <summary>Returns the text of the embedded file <paramref name="fileName"/> (for example
        /// <c>"BudgetP1RupeesAndMoneyColumns_01_domains.sql"</c>). Throws when it is absent.</summary>
        public static string Read(string fileName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            var assembly = typeof(SqlResource).Assembly;
            using var stream = assembly.GetManifestResourceStream(Prefix + fileName)
                ?? throw new InvalidOperationException($"Embedded SQL resource '{fileName}' not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>Names of every embedded SQL file (used by tests).</summary>
        public static IEnumerable<string> Names() =>
            typeof(SqlResource).Assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith(Prefix, StringComparison.Ordinal))
                .Select(n => n[Prefix.Length..]);
    }
}
