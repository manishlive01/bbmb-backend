using System.Net;
using System.Text;
using System.Text.Json;
using BBMB.BudgetService.Authorization.RoleSource;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
namespace BBMB.BudgetService.Tests.Authorization;
/// <summary>
/// <see cref="HttpSsoRoleClient"/>: the request it sends to Auth SSO and the tolerant parse of the response
/// (the response shape is an assumption until confirmed against Auth SSO). A stub handler replaces the network.
/// </summary>
public sealed class HttpSsoRoleClientTests
{
    private static readonly Guid A = Guid.NewGuid();

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? RequestBody { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Request = request;
            RequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }

    private static (HttpSsoRoleClient Client, StubHandler Stub) Client(HttpStatusCode status, string body)
    {
        var stub = new StubHandler(status, body);
        var http = new HttpClient(stub) { BaseAddress = new Uri("https://sso.example.invalid/sso-api/") };
        return (new HttpSsoRoleClient(http, NullLogger<HttpSsoRoleClient>.Instance), stub);
    }

    [Fact]
    public async Task Posts_the_project_id_in_the_body_with_the_forwarded_bearer_token()
    {
        var project = Guid.NewGuid();
        var (client, stub) = Client(HttpStatusCode.OK, $"[{{\"roleId\":\"{A}\",\"roleName\":\"DDO\",\"isActive\":true}}]");

        var r = await client.GetProjectRolesAsync(project, "tok");

        r.Roles.Should().ContainSingle().Which.Should().Be(new SsoRole(A, "DDO", true));
        stub.Request!.Method.Should().Be(HttpMethod.Post);
        stub.Request.RequestUri!.ToString().Should().Be("https://sso.example.invalid/sso-api/api/roles/by-project");
        stub.Request.Headers.Authorization!.ToString().Should().Be("Bearer tok");
        JsonDocument.Parse(stub.RequestBody!).RootElement.GetProperty("projectId").GetGuid().Should().Be(project);
    }

    [Theory]
    [InlineData("[{\"RoleId\":\"{A}\",\"RoleName\":\"DDO\",\"IsActive\":false}]", false)]           // PascalCase root array
    [InlineData("{\"data\":[{\"roleId\":\"{A}\",\"roleName\":\"DDO\",\"isActive\":false}]}", false)] // wrapped in data
    [InlineData("{\"Data\":{\"roles\":[{\"id\":\"{A}\",\"name\":\"DDO\",\"active\":true}]}}", true)] // nested, aliases
    [InlineData("[{\"roleId\":\"{A}\",\"roleName\":\"DDO\",\"isActive\":\"false\"}]", false)]      // string flag
    [InlineData("[{\"roleId\":\"{A}\",\"roleName\":\"DDO\",\"isActive\":0}]", false)]            // numeric flag
    public void Parses_camel_and_Pascal_case_and_common_wrappers(string json, bool active)
    {
        var roles = HttpSsoRoleClient.Parse(JsonDocument.Parse(json.Replace("{A}", A.ToString())).RootElement);

        roles.Should().ContainSingle().Which.Should().Be(new SsoRole(A, "DDO", active));
    }

    [Fact]
    public void Skips_rows_without_a_guid_id_or_a_name_and_rejects_an_unrecognised_body()
    {
        HttpSsoRoleClient.Parse(JsonDocument.Parse($"[{{\"roleId\":\"x\",\"roleName\":\"DDO\"}},{{\"roleId\":\"{A}\"}}]").RootElement)
            .Should().BeEmpty();
        HttpSsoRoleClient.Parse(JsonDocument.Parse("{\"message\":\"ok\"}").RootElement).Should().BeNull();
    }

    // Fail closed (review finding 4): a missing or unreadable active flag is never read as active. The whole body is
    // refused, so the sync reports SSO_SYNC_FAILED and changes nothing.
    [Theory]
    [InlineData("[{\"roleId\":\"{A}\",\"roleName\":\"DDO\"}]")]                          // flag missing
    [InlineData("[{\"roleId\":\"{A}\",\"roleName\":\"DDO\",\"status\":\"Inactive\"}]")]     // another field
    [InlineData("[{\"roleId\":\"{A}\",\"roleName\":\"DDO\",\"isActive\":\"Inactive\"}]")]   // unreadable string
    [InlineData("[{\"roleId\":\"{A}\",\"roleName\":\"DDO\",\"isActive\":2}]")]             // unreadable number
    [InlineData("[{\"roleId\":\"{A}\",\"roleName\":\"DDO\",\"isActive\":null}]")]          // null
    public void A_missing_or_unreadable_active_flag_rejects_the_whole_body(string json)
    {
        HttpSsoRoleClient.Parse(JsonDocument.Parse(json.Replace("{A}", A.ToString())).RootElement).Should().BeNull();
    }

    [Fact]
    public async Task An_unrecognised_body_is_a_failure()
    {
        var (client, _) = Client(HttpStatusCode.OK, $"[{{\"roleId\":\"{A}\",\"roleName\":\"DDO\"}}]");

        var r = await client.GetProjectRolesAsync(Guid.NewGuid(), "tok");

        r.Roles.Should().BeNull();
        r.Error.Should().Be("UNRECOGNISED_BODY");
    }

    [Fact]
    public async Task A_non_success_status_is_a_failure()
    {
        var (client, _) = Client(HttpStatusCode.Unauthorized, "{}");

        var r = await client.GetProjectRolesAsync(Guid.NewGuid(), "tok");

        r.Roles.Should().BeNull();
        r.Error.Should().Be("HTTP_401");
    }
}
