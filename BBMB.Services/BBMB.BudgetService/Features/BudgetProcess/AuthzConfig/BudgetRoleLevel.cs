using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig
{
    /// <summary>
    /// The ONE place that turns a role's <c>levels</c> master row into the Budget office level code
    /// (<see cref="OfficeLevel"/>: DIVISION, CIRCLE, CE, SECRETARIAT, FINANCE, IT_CELL) that
    /// <c>BudgetOfficeScope</c>, <c>ChargeResolver</c> and the actor read, and that decides whether a role is
    /// configured. The config snapshot, the admin list/configure endpoints and the migration's backfill
    /// (through <see cref="SqlAliasValues"/>) all use <see cref="Aliases"/>: the level's name, trimmed and
    /// compared case-insensitively against an explicit alias set. A level whose name is not in the set is "not a
    /// Budget office level" and a role on it stays unconfigured.
    /// <para>
    /// <b><c>levels.status</c> is deliberately ignored.</b> That flag belongs to the hierarchy master (office
    /// management screens); on the live database every row carries <c>status = false</c>. Budget authorisation
    /// is decided by the Budget-owned columns only (see <see cref="IsAuthorisable"/>).
    /// </para>
    /// <para>
    /// <b>Known limitation (review finding 8):</b> the level name is the authorisation key. Renaming a
    /// <c>levels</c> row (this service has no edit endpoint for it, but another system may) de-authorises every
    /// role on that level, and <c>budget.fn_config_version()</c> does not see the rename because <c>levels</c> has
    /// no audit trigger and is not part of it, so the cached snapshot keeps the old result until another config
    /// write or a restart. Removing this needs a code column on <c>levels</c> or an explicit mapping table.
    /// </para>
    /// </summary>
    public static class BudgetRoleLevel
    {
        /// <summary>
        /// <c>levels.name</c> (upper-cased, trimmed) → Budget office level code. The hierarchy master names its
        /// rows 'CE', 'CR' and 'Div'; the Budget-only levels use their code as the name.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> Aliases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["CE"] = OfficeLevel.Ce,
                ["CR"] = OfficeLevel.Circle,
                ["CIRCLE"] = OfficeLevel.Circle,
                ["DIV"] = OfficeLevel.Division,
                ["DIVISION"] = OfficeLevel.Division,
                ["FINANCE"] = OfficeLevel.Finance,
                ["SECRETARIAT"] = OfficeLevel.Secretariat,
                ["IT_CELL"] = OfficeLevel.ItCell,
            };

        /// <summary>
        /// Budget office levels that have no row in the hierarchy master. The migration inserts each one into
        /// <c>budget.levels</c> by this name, only where no row of that name exists.
        /// </summary>
        public static readonly IReadOnlyList<string> BudgetOnlyLevelNames =
            [OfficeLevel.Finance, OfficeLevel.Secretariat, OfficeLevel.ItCell];

        /// <summary>The Budget office level code for a levels row name, or null when the name is not an alias.</summary>
        public static string? ToOfficeLevelCode(string? levelName)
        {
            if (string.IsNullOrWhiteSpace(levelName))
                return null;
            return Aliases.TryGetValue(levelName.Trim(), out var code) ? code : null;
        }

        /// <summary>
        /// <see cref="Aliases"/> as a SQL <c>VALUES</c> list of (alias, code), for the migration backfill, so the
        /// SQL and the code cannot disagree. Every entry is a fixed identifier (letters and underscore).
        /// </summary>
        public static string SqlAliasValues()
        {
            foreach (var (alias, code) in Aliases)
                if (!IsIdentifier(alias) || !IsIdentifier(code))
                    throw new InvalidOperationException($"Level alias '{alias}' -> '{code}' is not an identifier.");
            return "VALUES " + string.Join(", ",
                Aliases.OrderBy(a => a.Key, StringComparer.Ordinal).Select(a => $"('{a.Key.ToUpperInvariant()}', '{a.Value}')"));
        }

        private static bool IsIdentifier(string s) => s.Length > 0 && s.All(c => c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_');

        /// <summary>
        /// D2: a role is authorised only when it is active in Budget, active in Auth SSO, has an office level that
        /// resolves to a Budget office level, and holds at least one active right. Anything else is
        /// unconfigured and gets no rights, no office and no acting-role candidacy.
        /// </summary>
        public static bool IsAuthorisable(bool isActive, bool ssoIsActive, string? officeLevelCode, bool hasActiveRight) =>
            isActive && ssoIsActive && officeLevelCode is not null && hasActiveRight;
    }
}
