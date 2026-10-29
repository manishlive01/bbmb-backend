using Microsoft.AspNetCore.Authorization;

namespace BBMB.BudgetService.Authorization.Requirements
{
    public class ProjectRoleRequirement : IAuthorizationRequirement
    {
        public string ProjectName { get; }
        public string[] AllowedRoles { get; }

        public ProjectRoleRequirement(string projectName, string[] roles)
        {
            ProjectName = projectName;
            AllowedRoles = roles;
        }
    }
}
