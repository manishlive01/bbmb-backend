using BBMB.BudgetService.Features.BudgetProcess.DemandLines;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.UploadWorksDemand;
using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Services.Attachments;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Settings;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.DemandLines;

/// <summary>
/// P7-01 (design §12 P2 exit, §8.5 F-08; RTM backend rows TC-BUD-BE-*). The works-demand upload (F-08) is
/// all-or-nothing: a single bad data row fails the whole import, the row is named by its sheet position, and
/// nothing is persisted (BUD-100, BUD-104). This is the one P2-exit bullet that had no end-to-end test; the rest
/// of the P2 exit sweep is already covered and the mapping is recorded in budget-backend-api-progress.json
/// (item P7-01 evidence).
///
/// The test drives UploadWorksDemandHandler with a real xlsx (the production parser is EPPlus) carrying the F-07
/// template headings, several good rows and one bad row, as a FixedScanner(Clean) and a temp-folder file store,
/// then asserts the refusal lists the bad row and that no demand line, upload batch or attachment row was
/// written. Method name carries the BUD token so generate_rtm_bud.py auto-links it.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class UploadWorksDemandAtomicityTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private string _root = default!;
    private FileStorageService _storage = default!;
    private string _accountCode = default!;
    private static readonly IConfiguration Config = new ConfigurationBuilder().Build();

    public async Task InitializeAsync()
    {
        _w = await SpineWorld.CreateAsync(fixture, $"upl_{Guid.NewGuid():N}"[..20]);
        _root = Path.Combine(Path.GetTempPath(), $"bbmb-upl-{Guid.NewGuid():N}");
        _storage = new FileStorageService(Options.Create(new FileSettings { FolderPath = _root }));
        await using var ctx = _w.NewContext();
        _accountCode = await ctx.HeadOfAccounts.Where(h => h.AccountCodeId == _w.AccountCodeId)
            .Select(h => h.CompleteCode!).SingleAsync();
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        return Task.CompletedTask;
    }

    private sealed class FixedScanner : IAttachmentScanner
    {
        public Task<ScanResult> ScanAsync(Stream content, CancellationToken ct) =>
            Task.FromResult(new ScanResult(ScanVerdict.Clean, "TEST 1.0", null));
    }

    /// <param name="rows">(accountCode, nameOfWork, rbe, obeNext) per data row.</param>
    private static IFormFile BuildWorkbook((string Code, string Name, decimal Rbe, decimal Obe)[] rows)
    {
        ExcelPackage.License.SetNonCommercialPersonal("BBMB");
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("demand");
        for (var c = 0; c < UploadTemplateColumns.Headings.Count; c++)
            sheet.Cells[1, c + 1].Value = UploadTemplateColumns.Headings[c];
        for (var i = 0; i < rows.Length; i++)
        {
            var r = i + 2;
            sheet.Cells[r, 1].Value = i + 1; // Sr. No. (not read)
            sheet.Cells[r, UploadTemplateColumns.AccountCode].Value = rows[i].Code;
            sheet.Cells[r, UploadTemplateColumns.NameOfWork].Value = rows[i].Name;
            sheet.Cells[r, UploadTemplateColumns.Rbe].Value = (double)rows[i].Rbe;
            sheet.Cells[r, UploadTemplateColumns.ObeNext].Value = (double)rows[i].Obe;
        }
        var bytes = package.GetAsByteArray();
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "demand.xlsx");
    }

    private async Task<Response> UploadAsync(IFormFile file)
    {
        await using var ctx = _w.NewContext();
        var handler = new UploadWorksDemandHandler(
            ctx, _w.DivisionA, new BudgetActionScope(ctx, _w.DivisionA), TestServices.OfficeScope(ctx),
            TestServices.AllowAll(), new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, Config,
            new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx), new BudgetSettingsReader(ctx),
            new FixedScanner(), _storage, NullLogger<UploadWorksDemandHandler>.Instance);
        return await handler.Handle(new UploadWorksDemandCommand(_w.WorksProposalAId, file), default);
    }

    private async Task<(int Lines, int Batches, int Attachments)> CountsAsync()
    {
        await using var ctx = _w.NewContext();
        return (
            await ctx.DemandLines.CountAsync(l => l.ProposalId == _w.WorksProposalAId),
            await ctx.UploadBatches.CountAsync(b => b.ProposalId == _w.WorksProposalAId),
            await ctx.BudgetAttachments.CountAsync());
    }

    [Fact]
    public async Task BUD100_BUD104_OneBadRowFailsTheWholeUploadAndPersistsNothing()
    {
        // Four well-formed rows and one bad row (row 4 of 5 carries an account code that is in no Manual -> M-05).
        // The bad row must fail the whole import; no line, batch or attachment may survive.
        var file = BuildWorkbook([
            (_accountCode, "Canal lining reach 1", 100, 10),
            (_accountCode, "Canal lining reach 2", 200, 20),
            (_accountCode, "Canal lining reach 3", 300, 30),
            ("9999-99-999-99-999-999", "Bad row", 400, 40), // unknown account code
            (_accountCode, "Canal lining reach 5", 500, 50),
        ]);

        var before = await CountsAsync();
        before.Should().Be((0, 0, 0));

        var result = await UploadAsync(file);

        result.IsSucced.Should().BeFalse("one bad row fails the whole upload (all-or-nothing, BUD-100)");
        result.Should().BeOfType<Response<RuleFailuresDto>>();
        var failures = ((Response<RuleFailuresDto>)result).Data!.Failures;
        failures.Should().Contain(f => f.SerialNo == 5 && f.MessageId == "M-05",
            "the bad row is named by its sheet row (row 5: header + four data rows) and carries its message code");

        var after = await CountsAsync();
        after.Should().Be((0, 0, 0), "nothing is persisted when the import is refused");

        // No orphaned file was left in the store either (the handler stores the attachment only after the rules pass).
        var dir = Path.Combine(_root, "BudgetAttachments");
        (Directory.Exists(dir) ? Directory.GetFiles(dir).Length : 0).Should().Be(0);
    }

    [Fact]
    public async Task BUD098_AllGoodRowsUploadAtomicallyAsOneBatch()
    {
        // The companion positive: a clean file of three rows commits all three as one batch with one attachment,
        // proving the atomic unit is the whole file (not row-by-row).
        var file = BuildWorkbook([
            (_accountCode, "Reach 1", 100, 10),
            (_accountCode, "Reach 2", 200, 20),
            (_accountCode, "Reach 3", 300, 30),
        ]);

        var result = await UploadAsync(file);
        result.IsSucced.Should().BeTrue(result.Message);

        var after = await CountsAsync();
        after.Lines.Should().Be(3);
        after.Batches.Should().Be(1);
        after.Attachments.Should().Be(1);
    }
}
