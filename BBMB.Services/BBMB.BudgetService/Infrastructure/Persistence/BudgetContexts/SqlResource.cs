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

        /// <summary>
        /// The migration-actor fragment (design §11.1 rule 8) for <paramref name="migrationName"/>. Every
        /// data-shaping step runs it first. For a gated migration (§11.1 rule 9) the gate's recorded decision
        /// becomes <c>budget.justification</c>.
        /// </summary>
        public static string MigrationActor(string migrationName, string? gate = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(migrationName);
            if (!System.Text.RegularExpressions.Regex.IsMatch(migrationName, "^[A-Za-z0-9_]+$"))
                throw new ArgumentException("Migration name must be an identifier.", nameof(migrationName));
            if (gate is not null && !System.Text.RegularExpressions.Regex.IsMatch(gate, "^[a-z0-9_]+$"))
                throw new ArgumentException("Gate must be an identifier.", nameof(gate));

            using var stream = typeof(SqlResource).Assembly.GetManifestResourceStream("BBMB.BudgetService.MigrationActor.sql")
                ?? throw new InvalidOperationException("Embedded MigrationActor.sql not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd()
                .Replace("{{MIGRATION}}", migrationName, StringComparison.Ordinal)
                .Replace("{{JUSTIFICATION}}",
                    gate is null ? "''" : $"coalesce(current_setting('bbmb_gate.{gate}', true), '')",
                    StringComparison.Ordinal);
        }

        /// <summary>Names of every embedded SQL file (used by tests).</summary>
        public static IEnumerable<string> Names() =>
            typeof(SqlResource).Assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith(Prefix, StringComparison.Ordinal))
                .Select(n => n[Prefix.Length..]);
    }
}
