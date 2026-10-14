using Microsoft.AspNetCore.Authorization;

namespace Auth.Authorization.Requirements
{
    /// <summary>
    /// Policy <c>BudgetRight:&lt;key&gt;</c> (design §4.4). Carries the right key only; the roles holding the
    /// right are read from <see cref="BudgetRightsOptions"/> at each evaluation, never baked into the policy
    /// (BUD-033, BUD-242).
    /// </summary>
    public sealed record BudgetRightRequirement(string RightKey) : IAuthorizationRequirement
    {
        public const string PolicyPrefix = "BudgetRight:";

        public static string Policy(string rightKey) => PolicyPrefix + rightKey;
    }
}
