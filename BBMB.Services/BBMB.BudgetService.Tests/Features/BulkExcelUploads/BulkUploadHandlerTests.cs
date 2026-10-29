using BBMB.BudgetService.Features.BulkExcelUploads.BulkUploads;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OfficeOpenXml;

namespace BBMB.BudgetService.Tests.Features.BulkExcelUploads;

/// <summary>Design §3 D-11, §9.3 (BulkExcelUploads row), §10 (bulk rows).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BulkUploadHandlerTests(BudgetDatabaseFixture fixture)
{
    private static IFormFile Sheet(params object?[][] rows)
    {
        ExcelPackage.License.SetNonCommercialPersonal("BBMB");
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Sheet1");
        for (var r = 0; r < rows.Length; r++)
            for (var c = 0; c < rows[r].Length; c++)
                ws.Cells[r + 1, c + 1].Value = rows[r][c];
        var bytes = package.GetAsByteArray();
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "upload.xlsx");
    }

    private BulkUploadHandler Handler(params string[] allowedPolicies) => Handler(FakeBudgetActorSource.None(), allowedPolicies);

    private BulkUploadHandler Handler(FakeBudgetActorSource actor, params string[] allowedPolicies)
    {
        var ctx = fixture.NewBudgetContext();
        return new BulkUploadHandler(
            ctx,
            new FakeAuthorizationService(allowedPolicies),
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            NullLogger<BulkUploadHandler>.Instance,
            actor,
            new BBMB.BudgetService.Shared.Services.BudgetActors.BudgetActionScope(ctx, actor));
    }

    private async Task EnsureProjectAsync()
    {
        await using var ctx = fixture.NewBudgetContext();
        await ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO budget.projects (proj_code, proj_desc, modification_date, user_code) " +
            "VALUES ('T', 'Test project', now(), 'TEST') ON CONFLICT DO NOTHING");
    }

    private static object?[] Header => ["CECode", "Name", "ProjectCode", "Desc", "Location"];

    private async Task<int> CeCountAsync(params int[] codes)
    {
        await using var ctx = fixture.NewBudgetContext();
        return await ctx.CEManagements.CountAsync(c => codes.Contains(c.CECode));
    }

    [Fact]
    public async Task D11_workmaster_is_an_invalid_upload_type()
    {
        var r = await Handler("BudgetRight:F01.Create", "BudgetRight:Offices.Load")
            .Handle(new BulkUploadCommand(Sheet(["x"], ["y"]), "workmaster"), default);
        r.IsSucced.Should().BeFalse();
        r.Message.Should().Be("Invalid UploadType");
    }

    [Fact]
    public async Task BUD030_cemaster_without_Offices_Load_is_refused_with_M20_and_nothing_saved()
    {
        await EnsureProjectAsync();
        var r = await Handler("BudgetRight:F01.Create")
            .Handle(new BulkUploadCommand(Sheet(Header, [970001, "CE A", "T", "d", "l"]), "cemaster"), default);
        r.IsSucced.Should().BeFalse();
        r.MessageId.Should().Be("M-20");
        (await CeCountAsync(970001)).Should().Be(0);
    }

    [Fact]
    public async Task BUD030_cemaster_with_Offices_Load_saves_the_rows()
    {
        await EnsureProjectAsync();
        var r = await Handler("BudgetRight:Offices.Load")
            .Handle(new BulkUploadCommand(Sheet(Header, [970011, "CE A", "T", "d", "l"], [970012, "CE B", "T", null, null]), "cemaster"), default);
        r.IsSucced.Should().BeTrue(r.Message);
        (await CeCountAsync(970011, 970012)).Should().Be(2);
    }

    [Fact]
    public async Task BUD012_sheet_validation_message_is_returned_and_nothing_is_saved()
    {
        await EnsureProjectAsync();
        var r = await Handler("BudgetRight:Offices.Load")
            .Handle(new BulkUploadCommand(Sheet(Header, [970021, "CE A", "T", "d", "l"], ["bad", "CE B", "T", "d", "l"]), "cemaster"), default);
        r.IsSucced.Should().BeFalse();
        r.Message.Should().Be("Invalid CECode at row 3");
        (await CeCountAsync(970021)).Should().Be(0);
    }

    [Fact]
    public async Task BUD012_unreadable_file_returns_the_existing_message_not_parser_text()
    {
        var bytes = "not an excel file"u8.ToArray();
        var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "upload.xlsx");
        var r = await Handler("BudgetRight:Offices.Load").Handle(new BulkUploadCommand(file, "cemaster"), default);
        r.IsSucced.Should().BeFalse();
        r.Message.Should().Be("Invalid Excel file");
    }
}
