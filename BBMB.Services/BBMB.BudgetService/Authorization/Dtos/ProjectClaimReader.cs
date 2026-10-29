using System.Security.Claims;
using System.Text.Json;

namespace BBMB.BudgetService.Authorization.Dtos
{
    /// <summary>
    /// Reads the JSON <c>projects</c> claims with the same parsing as <c>ProjectRoleHandler</c>: one claim per
    /// project, deserialised case-insensitively into <see cref="ProjectRoleTokenDto"/>; malformed claims are
    /// ignored (design §4.4).
    /// </summary>
    public static class ProjectClaimReader
    {
        public const string ClaimType = "projects";

        private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

        public static IReadOnlyList<ProjectRoleTokenDto> Read(ClaimsPrincipal? user)
        {
            var result = new List<ProjectRoleTokenDto>();
            if (user is null)
                return result;

            foreach (var claim in user.FindAll(ClaimType))
            {
                try
                {
                    var project = JsonSerializer.Deserialize<ProjectRoleTokenDto>(claim.Value, Options);
                    if (project != null)
                        result.Add(project);
                }
                catch (JsonException)
                {
                    // ignore malformed claim, as ProjectRoleHandler does
                }
            }
            return result;
        }

        /// <summary>Distinct role names the user holds in <paramref name="projectName"/>.</summary>
        public static IReadOnlyList<string> RolesIn(ClaimsPrincipal? user, string projectName) =>
            RolesInProject(user, null, projectName);

        /// <summary>
        /// Distinct role names the user holds in the Budget project, matched by <paramref name="projectId"/>
        /// first (the correct identity — names collide, <c>Budget</c> vs <c>Budget-Staging</c>), falling back
        /// to <paramref name="projectName"/> when the id is absent (design §3, OQ-2). Roles under any other
        /// project are never returned.
        /// </summary>
        public static IReadOnlyList<string> RolesInProject(ClaimsPrincipal? user, Guid? projectId, string? projectName)
        {
            var projects = Read(user);

            IEnumerable<ProjectRoleTokenDto> matched;
            if (projectId is { } id && id != Guid.Empty && projects.Any(p => p.ProjectId == id))
                matched = projects.Where(p => p.ProjectId == id);
            else if (!string.IsNullOrWhiteSpace(projectName))
                matched = projects.Where(p => string.Equals(p.ProjectName, projectName, StringComparison.OrdinalIgnoreCase));
            else
                return [];

            return matched
                .SelectMany(p => p.Roles)
                .Select(r => r.RoleName)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
