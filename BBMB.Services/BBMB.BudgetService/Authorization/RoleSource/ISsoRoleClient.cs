namespace BBMB.BudgetService.Authorization.RoleSource
{
    /// <summary>One role of a project as Auth SSO reports it.</summary>
    public sealed record SsoRole(Guid RoleId, string RoleName, bool IsActive);

    /// <summary>The outcome of reading a project's roles from Auth SSO. <see cref="Roles"/> is null on failure.</summary>
    public sealed record SsoRoleFetchResult(IReadOnlyList<SsoRole>? Roles, string? Error)
    {
        public static SsoRoleFetchResult Ok(IReadOnlyList<SsoRole> roles) => new(roles, null);
        public static SsoRoleFetchResult Fail(string error) => new(null, error);
    }

    /// <summary>
    /// Reads the role catalogue of one Auth SSO project (<c>POST {SSO:Authority}api/roles/by-project</c>, body
    /// <c>{"projectId": ...}</c>), forwarding the calling administrator's bearer token. Behind an interface so the
    /// role sync can be tested with a fake. Read-only: it never writes to Auth SSO.
    /// </summary>
    public interface ISsoRoleClient
    {
        Task<SsoRoleFetchResult> GetProjectRolesAsync(Guid projectId, string bearerToken, CancellationToken ct = default);
    }
}
