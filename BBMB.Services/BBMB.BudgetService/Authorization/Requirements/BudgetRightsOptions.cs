namespace Auth.Authorization.Requirements
{
    /// <summary>
    /// Configuration section <c>BudgetRights</c>: right key → roles holding it (design §4.4, SRS §10). This is
    /// the module's reading of the portal Role and Rights matrix until the portal publishes rights (OI-05).
    /// Role names are placeholders until the token's role names are confirmed (OI-06).
    /// </summary>
    public sealed class BudgetRightsOptions
    {
        public const string SectionName = "BudgetRights";

        /// <summary>The project name carried in the <c>projects</c> claim for this module.</summary>
        public string ProjectName { get; set; } = "Budget";

        public Dictionary<string, string[]> Rights { get; set; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Acting role → T-13 office level the role acts from (design §4.4 "Office"): DIVISION, CIRCLE and CE
        /// resolve through the charge's division_id, cr_id and ce_id; FINANCE, SECRETARIAT and IT_CELL by role,
        /// because employee_charge carries no Finance or Secretariat office (OI-08). Held in configuration with
        /// the rights, so no role name is fixed in code (BUD-033).
        /// </summary>
        public Dictionary<string, string> RoleOfficeLevels { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
