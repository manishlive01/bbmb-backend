using BBMB.BudgetService.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Authorization.PolicyProvider
{
    public class ProjectRolePolicyProvider
        : DefaultAuthorizationPolicyProvider
    {
        public ProjectRolePolicyProvider(
            IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?>
            GetPolicyAsync(string policyName)
        {
            var existingPolicy = await base.GetPolicyAsync(policyName);

            if (existingPolicy != null)
                return existingPolicy;

            // Format: BudgetRight:<key> (design §4.4). Checked ahead of the ProjectName:Roles parse, which
            // would otherwise read "BudgetRight" as a project name. The policy carries the key only.
            if (policyName.StartsWith(BudgetRightRequirement.PolicyPrefix, StringComparison.Ordinal))
            {
                var key = policyName[BudgetRightRequirement.PolicyPrefix.Length..];
                if (string.IsNullOrWhiteSpace(key))
                    return null;
                return new AuthorizationPolicyBuilder()
                    .AddRequirements(new BudgetRightRequirement(key))
                    .Build();
            }
            // Format: ProjectName:Role1,Role2
            if (!policyName.Contains(":"))
                return null;

            var parts = policyName.Split(':');

            if (parts.Length != 2)
                return null;

            var projectName = parts[0];
            var roles = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries);

            var policyBuilder = new AuthorizationPolicyBuilder();
            policyBuilder.AddRequirements(
                new ProjectRoleRequirement(projectName, roles));

            return policyBuilder.Build();
        }
    }
}
