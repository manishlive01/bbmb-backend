using BBMB.BudgetService.Features.HeadOfAccounts.GetById;
using BBMB.BudgetService.Features.HeadOfAccounts.ParseAccountCode;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
namespace BBMB.BudgetService.Tests.Features.HeadOfAccounts;
/// <summary>Design §9.3 HeadOfAccounts reads row: the T-07 key, complete code, not-for-use flag and origin are exposed.</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class HeadOfAccountReadTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"hoa_{Guid.NewGuid():N}"[..20], withCycle: false);
    public Task DisposeAsync() => Task.CompletedTask;
    private string Expected => $"{_w.Segments.Major}-{_w.Segments.SubMajor}-{_w.Segments.Minor}-{_w.Segments.SubMinor}-{_w.Segments.Detail}-{_w.Segments.Object}";

    [Fact]
    public async Task S93_GetById_returns_accountCodeId_and_completeCode()
    {
        var s = _w.Segments;
        await using var ctx = _w.NewContext();
        var r = await new GetHeadOfAccountByIdHandler(ctx).Handle(new GetHeadOfAccountByIdQuery(s.Major, s.SubMajor, s.Minor, s.SubMinor, s.Detail, s.Object), default);
        r.AccountCodeId.Should().Be(_w.AccountCodeId);
        r.CompleteCode.Should().Be(Expected).And.MatchRegex(@"^\d{4}-\d{2}-\d{3}-\d{2}-\d{3}-\d{3}$");
        r.NotForUse.Should().BeFalse();
        r.Origin.Should().Be("LEGACY");
    }

    [Fact]
    public async Task S93_Parse_returns_the_code_the_segments_form()
    {
        await using var ctx = _w.NewContext();
        var r = await new ParseAccountCodeHandler(ctx).Handle(new ParseAccountCodeQuery(Expected), default);
        r.AccountCodeId.Should().Be(_w.AccountCodeId);
        r.CompleteCode.Should().Be(Expected);
    }
}
