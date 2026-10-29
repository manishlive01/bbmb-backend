using BBMB.BudgetService.Authorization.Dtos;
using BBMB.BudgetService.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace BBMB.BudgetService.Authorization.Handlers
{
    public class ProjectRoleHandler
        : AuthorizationHandler<ProjectRoleRequirement>
    {
        protected override Task HandleRequirementAsync(
     AuthorizationHandlerContext context,
     ProjectRoleRequirement requirement)
        {
            var projectClaims = context.User.FindAll("projects");

            if (!projectClaims.Any())
                return Task.CompletedTask;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var projects = new List<ProjectRoleTokenDto>();

            foreach (var claim in projectClaims)
            {
                try
                {
                    var project = JsonSerializer.Deserialize<ProjectRoleTokenDto>(
                        claim.Value, options);

                    if (project != null)
                        projects.Add(project);
                }
                catch
                {
                    // ignore malformed claim
                }
            }

            if (!projects.Any())
                return Task.CompletedTask;

            var matchedProject = projects.FirstOrDefault(p =>
                p.ProjectName.Equals(requirement.ProjectName,
                    StringComparison.OrdinalIgnoreCase));

            if (matchedProject == null)
                return Task.CompletedTask;

            if (matchedProject.Roles.Any(r =>
                requirement.AllowedRoles
                    .Contains(r.RoleName, StringComparer.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }


    }
}
