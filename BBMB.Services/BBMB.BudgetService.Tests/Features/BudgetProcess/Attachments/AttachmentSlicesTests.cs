using System.Security.Cryptography;
using System.Text.Json;
using BBMB.BudgetService.Features.BudgetProcess.Attachments.Download;
using BBMB.BudgetService.Features.BudgetProcess.Attachments.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload;
using BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload.Dtos;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Services.Attachments;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Settings;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Attachments;
/// <summary>Design §4.8, §8.2 attachment rows, §10 attachment rows (BUD-018, BUD-019, DDD T-22).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class AttachmentSlicesTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private string _root = default!;
    private FileStorageService _storage = default!;

    public async Task InitializeAsync()
    {
        _w = await SpineWorld.CreateAsync(fixture, $"att_{Guid.NewGuid():N}"[..20]);
        _root = Path.Combine(Path.GetTempPath(), $"bbmb-att-{Guid.NewGuid():N}");
        _storage = new FileStorageService(Options.Create(new FileSettings { FolderPath = _root }));
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
        return Task.CompletedTask;
    }

    private sealed class FixedScanner(ScanVerdict verdict) : IAttachmentScanner
    {
        public byte[]? Scanned { get; private set; }
        public async Task<ScanResult> ScanAsync(Stream content, CancellationToken ct)
        {
            using var copy = new MemoryStream();
            await content.CopyToAsync(copy, ct);
            Scanned = copy.ToArray();
            return new ScanResult(verdict, "TEST 1.0", verdict == ScanVerdict.Infected ? "Eicar-Signature" : null);
        }
    }

    private static byte[] Pdf(int size)
    {
        var bytes = new byte[size];
        "%PDF-1.7\n"u8.CopyTo(bytes);
        for (var i = 9; i < size; i++) bytes[i] = (byte)('a' + i % 26);
        return bytes;
    }

    private static readonly byte[] PngHead = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];

    private static IFormFile Form(byte[] bytes, string fileName) =>
        new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName);

    private async Task<UploadAttachmentResponse> UploadAsync(
        byte[] bytes, string fileName, string evidenceClass = "EVIDENCE", IAttachmentScanner? scanner = null,
        FakeBudgetActorSource? actor = null)
    {
        actor ??= _w.DivisionA;
        await using var ctx = _w.NewContext();
        return await new UploadAttachmentHandler(ctx, actor, new BudgetActionScope(ctx, actor),
                scanner ?? new FixedScanner(ScanVerdict.Clean), _storage, NullLogger<UploadAttachmentHandler>.Instance)
            .Handle(new UploadAttachmentCommand(Form(bytes, fileName), evidenceClass), default);
    }

    private async Task<BBMB.BudgetService.Shared.ResponseDTOs.Response<BBMB.BudgetService.Features.BudgetProcess.Attachments.Download.Dtos.AttachmentFileDto>>
        DownloadAsync(long id, FakeBudgetActorSource actor)
    {
        await using var ctx = _w.NewContext();
        return await new DownloadAttachmentHandler(ctx, actor, new AttachmentAccessRules(ctx, TestServices.OfficeScope(ctx)),
                _storage, NullLogger<DownloadAttachmentHandler>.Instance)
            .Handle(new DownloadAttachmentQuery(id), default);
    }

    private async Task<int> AttachmentRowsAsync()
    {
        await using var ctx = _w.NewContext();
        return await ctx.BudgetAttachments.CountAsync();
    }

    private string[] StoredFiles()
    {
        var dir = Path.Combine(_root, "BudgetAttachments");
        return Directory.Exists(dir) ? Directory.GetFiles(dir) : [];
    }

    // ------------------------------------------------------------------ upload

    [Fact]
    public async Task BUD019_file_of_exactly_10485760_bytes_is_accepted_and_stored_digest_returned_digest_and_T22_digest_agree()
    {
        var bytes = Pdf(10_485_760);
        var scanner = new FixedScanner(ScanVerdict.Clean);
        var r = await UploadAsync(bytes, "order.pdf", scanner: scanner);
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.SizeBytes.Should().Be(10_485_760);

        var expected = Convert.ToHexStringLower(SHA256.HashData(bytes));
        r.Data.Sha256.Should().Be(expected);
        scanner.Scanned.Should().Equal(bytes, "the scanner saw every byte from position 0");

        await using var ctx = _w.NewContext();
        var row = await ctx.BudgetAttachments.SingleAsync(a => a.AttachmentId == r.Data.AttachmentId);
        row.Sha256Digest.Should().Be(expected);
        row.ContentType.Should().Be("application/pdf");
        row.ScanEngine.Should().Be("TEST 1.0");
        await using var stored = _storage.OpenRead(FileType.BudgetAttachment, row.StorageKey.ToString())!;
        Convert.ToHexStringLower(await SHA256.HashDataAsync(stored)).Should().Be(expected, "the stored bytes are the bytes hashed");
    }

    [Fact]
    public async Task BUD019_one_byte_over_10485760_is_refused_M24_and_nothing_is_stored()
    {
        var r = await UploadAsync(Pdf(10_485_761), "order.pdf");
        r.IsSucced.Should().BeFalse();
        r.MessageId.Should().Be(BudgetMessages.M24);
        (await AttachmentRowsAsync()).Should().Be(0);
        StoredFiles().Should().BeEmpty();
    }

    [Fact]
    public async Task BUD019_EVIDENCE_with_xlsx_is_refused_M24_but_SUPPORTING_xlsx_is_accepted()
    {
        var xlsx = new byte[] { 0x50, 0x4B, 0x03, 0x04, 1, 2, 3, 4 };
        (await UploadAsync(xlsx, "sheet.xlsx", "EVIDENCE")).MessageId.Should().Be(BudgetMessages.M24);
        (await UploadAsync(xlsx, "sheet.xlsx", "SUPPORTING")).IsSucced.Should().BeTrue();
    }

    [Fact]
    public async Task BUD019_pdf_name_with_png_bytes_is_refused_M24()
    {
        (await UploadAsync(PngHead, "copy.pdf")).MessageId.Should().Be(BudgetMessages.M24);
        (await UploadAsync(PngHead, "copy.png")).IsSucced.Should().BeTrue("the content agrees with the extension");
    }

    [Fact]
    public async Task BUD019_extension_outside_the_T22_list_is_refused_M24()
    {
        (await UploadAsync(Pdf(100), "run.exe", "SUPPORTING")).MessageId.Should().Be(BudgetMessages.M24);
    }

    [Fact]
    public async Task BUD018_infected_file_is_refused_M24_and_stores_no_file_and_no_row()
    {
        var r = await UploadAsync(Pdf(2048), "order.pdf", scanner: new FixedScanner(ScanVerdict.Infected));
        r.MessageId.Should().Be(BudgetMessages.M24);
        r.ScannerUnavailable.Should().BeFalse("malware is a 409, not a 503");
        r.Message.Should().Contain("malware scan");
        (await AttachmentRowsAsync()).Should().Be(0);
        StoredFiles().Should().BeEmpty();
    }

    [Fact]
    public async Task BUD018_scanner_unavailable_fails_closed_503_path_and_stores_no_file_and_no_row()
    {
        var r = await UploadAsync(Pdf(2048), "order.pdf", scanner: new FixedScanner(ScanVerdict.Unavailable));
        r.MessageId.Should().Be(BudgetMessages.M24);
        r.ScannerUnavailable.Should().BeTrue();
        JsonSerializer.Serialize(r).Should().NotContain("ScannerUnavailable", "the flag is for the endpoint only");
        (await AttachmentRowsAsync()).Should().Be(0);
        StoredFiles().Should().BeEmpty();
    }

    [Fact]
    public async Task BUD018_scanner_unavailable_from_the_production_registration_refuses_the_upload()
    {
        var r = await UploadAsync(Pdf(2048), "order.pdf", scanner: new UnavailableAttachmentScanner());
        r.ScannerUnavailable.Should().BeTrue();
        (await AttachmentRowsAsync()).Should().Be(0);
    }

    [Fact]
    public async Task M20_no_actor_uploads_nothing()
    {
        (await UploadAsync(Pdf(100), "order.pdf", actor: FakeBudgetActorSource.None())).MessageId.Should().Be(BudgetMessages.M20);
        StoredFiles().Should().BeEmpty();
    }

    [Fact]
    public async Task Upload_row_carries_the_actor_from_the_session_not_the_request()
    {
        var r = await UploadAsync(Pdf(100), "order.pdf");
        await using var ctx = _w.NewContext();
        var row = await ctx.BudgetAttachments.SingleAsync(a => a.AttachmentId == r.Data!.AttachmentId);
        row.CreatedBy.Should().Be(_w.DivisionA.Actor!.EmployeeId);
    }

    // ------------------------------------------------------------------ download

    [Fact]
    public async Task Download_by_the_uploader_serves_the_verified_bytes()
    {
        var bytes = Pdf(4096);
        var up = await UploadAsync(bytes, "order.pdf");
        var r = await DownloadAsync(up.Data!.AttachmentId, _w.DivisionA);
        r.IsSucced.Should().BeTrue(r.Message);
        r.Data!.ContentType.Should().Be("application/pdf");
        r.Data.FileName.Should().Be("order.pdf");
        using var served = new MemoryStream();
        await r.Data.Content.CopyToAsync(served);
        served.ToArray().Should().Equal(bytes);
    }

    [Fact]
    public async Task BUD019_tampered_stored_file_is_INTEGRITY_and_not_served()
    {
        var up = await UploadAsync(Pdf(4096), "order.pdf");
        await using (var ctx = _w.NewContext())
        {
            var key = (await ctx.BudgetAttachments.SingleAsync(a => a.AttachmentId == up.Data!.AttachmentId)).StorageKey;
            var path = Path.Combine(_root, "BudgetAttachments", key.ToString());
            var tampered = await File.ReadAllBytesAsync(path);
            tampered[100] ^= 0xFF;
            await File.WriteAllBytesAsync(path, tampered);
        }
        var r = await DownloadAsync(up.Data!.AttachmentId, _w.DivisionA);
        r.IsSucced.Should().BeFalse();
        r.MessageId.Should().Be(BudgetMessages.Integrity);
        r.Data.Should().BeNull("the file is not served");
    }

    [Fact]
    public async Task Download_of_a_missing_stored_file_is_INTEGRITY()
    {
        var up = await UploadAsync(Pdf(4096), "order.pdf");
        foreach (var f in StoredFiles()) File.Delete(f);
        (await DownloadAsync(up.Data!.AttachmentId, _w.DivisionA)).MessageId.Should().Be(BudgetMessages.Integrity);
    }

    [Fact]
    public async Task BUD031_unreferenced_attachment_of_another_officer_is_NOT_FOUND()
    {
        var up = await UploadAsync(Pdf(4096), "order.pdf");
        var other = await TestActor.CreateAsync(fixture, role: "DDO", officeId: _w.DivisionBOfficeId, officeLevel: "DIVISION",
            name: "Division B Officer", connectionString: _w.ConnectionString);
        (await DownloadAsync(up.Data!.AttachmentId, other)).MessageId.Should().Be(BudgetMessages.NotFound);
        (await DownloadAsync(999_999, _w.DivisionA)).MessageId.Should().Be(BudgetMessages.NotFound);
    }

    [Fact]
    public async Task BUD031_attachment_referenced_by_an_approval_is_visible_upward_and_not_across_Divisions()
    {
        var up = await UploadAsync(Pdf(4096), "aa.pdf");
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_CREATE");
        await using (c)
        {
            await using var h = new NpgsqlCommand(
                "INSERT INTO budget.admin_approval (work_id, division_office_id, serial_no, current_version_no) " +
                $"VALUES ({_w.WorkAId}, {_w.DivisionAOfficeId}, 1, 1) RETURNING admin_approval_id", c, tx);
            var aa = Convert.ToInt64(await h.ExecuteScalarAsync());
            await using var v = new NpgsqlCommand(
                "INSERT INTO budget.admin_approval_version (admin_approval_id, version_no, name_of_work, aa_reference_no, date_of_approval, " +
                "account_code_id, amount_sanctioned, aa_copy_attachment_id, competent_authority, exceeds_delegation) " +
                $"VALUES ({aa}, 1, 'Work', 'AA/9', current_date - 1, {_w.AccountCodeId}, 1000, {up.Data!.AttachmentId}, 'CE', false)", c, tx);
            await v.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        var circle = await TestActor.CreateAsync(fixture, role: "SE", officeId: _w.CircleOfficeId, officeLevel: "CIRCLE",
            name: "Circle Officer", connectionString: _w.ConnectionString);
        var divisionB = await TestActor.CreateAsync(fixture, role: "DDO", officeId: _w.DivisionBOfficeId, officeLevel: "DIVISION",
            name: "Division B Officer", connectionString: _w.ConnectionString);
        (await DownloadAsync(up.Data.AttachmentId, circle)).IsSucced.Should().BeTrue("the Circle sees Division A's approval");
        (await DownloadAsync(up.Data.AttachmentId, divisionB)).MessageId.Should().Be(BudgetMessages.NotFound);
    }

    // ------------------------------------------------------------------ routes and rights

    [Fact]
    public void S44_upload_route_carries_Attachment_Upload_and_download_route_Budget_View()
    {
        EndpointCatalog.Find("POST", "/api/budget-attachments")!.Policies.Should().Equal("BudgetRight:Attachment.Upload");
        EndpointCatalog.Find("GET", "/api/budget-attachments/{id:long}")!.Policies.Should().Equal("BudgetRight:Budget.View");
    }

    [Fact]
    public void S44_FIN_VIEW_and_IT_CELL_lack_Attachment_Upload_in_the_default_rights()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var roles = doc.RootElement.GetProperty("BudgetRights").GetProperty("Rights").GetProperty("Attachment.Upload")
            .EnumerateArray().Select(e => e.GetString()).ToList();
        roles.Should().BeEquivalentTo(["DDO", "SE", "CE", "SECRETARY", "AFA_BUDGET"]);
        roles.Should().NotContain("FIN_VIEW").And.NotContain("IT_CELL");
    }
}
