using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
namespace BBMB.BudgetService.Authorization.RoleSource
{
    /// <summary>
    /// Typed <see cref="HttpClient"/> for <c>POST {SSO:Authority}api/roles/by-project</c> (D3). The base address
    /// is <c>SSO:Authority</c>; the caller's bearer token is forwarded per request, never stored or logged.
    /// <para>
    /// <b>Response shape (assumption, not confirmed against the Auth SSO source):</b> an array of
    /// <c>{roleId, roleName, isActive}</c>, either at the root or under <c>data</c> / <c>roles</c> / <c>result</c>
    /// (any casing). Property names are matched case-insensitively, so camelCase and PascalCase both parse;
    /// <c>id</c>/<c>name</c> are accepted as aliases. The active flag (<c>isActive</c>, alias <c>active</c>) must be
    /// a JSON boolean, <c>"true"</c>/<c>"false"</c>, or <c>0</c>/<c>1</c>; a role with the flag missing or in any
    /// other form makes the whole body unrecognised (fail closed: never read as active). A row without a GUID
    /// role id or a name is skipped. Any non-2xx status or an unparseable body is a failure, so the sync changes
    /// nothing.
    /// </para>
    /// </summary>
    public sealed class HttpSsoRoleClient(HttpClient http, ILogger<HttpSsoRoleClient> logger) : ISsoRoleClient
    {
        public const string Path = "api/roles/by-project";

        public async Task<SsoRoleFetchResult> GetProjectRolesAsync(Guid projectId, string bearerToken, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(bearerToken);
            using var request = new HttpRequestMessage(HttpMethod.Post, Path)
            {
                Content = JsonContent.Create(new { projectId }),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            try
            {
                using var response = await http.SendAsync(request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Auth SSO roles/by-project answered {Status}", (int)response.StatusCode);
                    return SsoRoleFetchResult.Fail($"HTTP_{(int)response.StatusCode}");
                }
                await using var body = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(body, cancellationToken: ct);
                var roles = Parse(doc.RootElement);
                return roles is null ? SsoRoleFetchResult.Fail("UNRECOGNISED_BODY") : SsoRoleFetchResult.Ok(roles);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !ct.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Auth SSO roles/by-project call failed");
                return SsoRoleFetchResult.Fail("UNREACHABLE");
            }
        }

        /// <summary>Tolerant parse of the role list (see the class remarks). Null when no array is found.</summary>
        public static IReadOnlyList<SsoRole>? Parse(JsonElement root)
        {
            var array = FindArray(root);
            if (array is null)
                return null;
            var roles = new List<SsoRole>();
            foreach (var item in array.Value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;
                var idText = Get(item, "roleId") ?? Get(item, "id");
                var name = Get(item, "roleName") ?? Get(item, "name");
                if (!Guid.TryParse(idText, out var id) || string.IsNullOrWhiteSpace(name))
                    continue;
                // Fail closed: a role whose active flag is missing or unreadable is never assumed active. The
                // whole body is refused, so the sync changes nothing rather than storing a guess.
                if ((GetBool(item, "isActive") ?? GetBool(item, "active")) is not { } active)
                    return null;
                roles.Add(new SsoRole(id, name.Trim(), active));
            }
            return roles;
        }

        private static JsonElement? FindArray(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
                return root;
            if (root.ValueKind != JsonValueKind.Object)
                return null;
            foreach (var key in new[] { "data", "roles", "result" })
            {
                if (TryProperty(root, key, out var inner))
                {
                    if (inner.ValueKind == JsonValueKind.Array)
                        return inner;
                    if (inner.ValueKind == JsonValueKind.Object && FindArray(inner) is { } nested)
                        return nested;
                }
            }
            return null;
        }

        private static bool TryProperty(JsonElement obj, string name, out JsonElement value)
        {
            foreach (var p in obj.EnumerateObject())
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = p.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        private static string? Get(JsonElement obj, string name) =>
            TryProperty(obj, name, out var v) && v.ValueKind is JsonValueKind.String or JsonValueKind.Number
                ? v.ToString()
                : null;

        private static bool? GetBool(JsonElement obj, string name)
        {
            if (!TryProperty(obj, name, out var v))
                return null;
            return v.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String when bool.TryParse(v.GetString(), out var b) => b,
                JsonValueKind.Number when v.TryGetInt32(out var n) && n is 0 or 1 => n == 1,
                _ => null,
            };
        }
    }
}
