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
            Read(user)
                .Where(p => string.Equals(p.ProjectName, projectName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(p => p.Roles)
                .Select(r => r.RoleName)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
    }
}
