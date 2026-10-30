using System.Net;
using System.Net.Sockets;
using System.Text;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Services.Attachments;
using BBMB.BudgetService.Shared.Settings;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Tests.Shared.Services;

/// <summary>Attachment services of design §4.8 (P2-03): content detection, digest, scanner boundary, storage.</summary>
public sealed class AttachmentServicesTests
{
    private static readonly byte[] PdfBytes = "%PDF-1.7\n%test"u8.ToArray();
    private static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13];
    private static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0, 16];
    private static readonly byte[] TiffBytes = [0x49, 0x49, 0x2A, 0x00, 8, 0, 0, 0];
    private static readonly byte[] XlsxBytes = [0x50, 0x4B, 0x03, 0x04, 20, 0];

    [Theory]
    [InlineData("pdf", "application/pdf")]
    [InlineData("png", "image/png")]
    [InlineData("jpg", "image/jpeg")]
    [InlineData("jpeg", "image/jpeg")]
    [InlineData("tif", "image/tiff")]
    [InlineData("tiff", "image/tiff")]
    [InlineData("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public void BUD019_magic_bytes_detect_the_content_type(string ext, string expected)
    {
        var bytes = ext switch
        {
            "pdf" => PdfBytes, "png" => PngBytes, "jpg" or "jpeg" => JpegBytes, "tif" or "tiff" => TiffBytes, _ => XlsxBytes,
        };
        AttachmentContent.DetectContentType(ext, bytes).Should().Be(expected);
    }

    [Fact]
    public void BUD019_a_pdf_carrying_png_bytes_is_rejected()
    {
        AttachmentContent.DetectContentType("pdf", PngBytes).Should().BeNull();
        AttachmentContent.DetectContentType("png", PdfBytes).Should().BeNull();
    }

    [Fact]
    public void Extension_is_lower_cased_and_the_evidence_set_is_pdf_and_images_only()
    {
        AttachmentContent.ExtensionOf("Copy.PDF").Should().Be("pdf");
        AttachmentContent.EvidenceExtensions.Should().BeEquivalentTo(["pdf", "jpg", "jpeg", "png", "tif", "tiff"]);
        AttachmentContent.AllowedExtensions.Should().HaveCount(11);
        AttachmentContent.MaxSizeBytes.Should().Be(10_485_760);
    }

    [Fact]
    public async Task Sha256_is_lower_case_hex_of_64_characters()
    {
        var digest = await AttachmentContent.Sha256HexAsync(new MemoryStream("abc"u8.ToArray()), CancellationToken.None);
        digest.Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
    }

    /// <summary>A minimal clamd: answers VERSION, and INSTREAM with <paramref name="instreamReply"/>.</summary>
    private static (TcpListener Listener, Task Server) FakeClamd(string instreamReply, int connections = 2)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var server = Task.Run(async () =>
        {
            for (var i = 0; i < connections; i++)
            {
                using var client = await listener.AcceptTcpClientAsync();
                await using var s = client.GetStream();
                var cmd = new StringBuilder();
                int b;
                while ((b = s.ReadByte()) > 0) cmd.Append((char)b);
                if (cmd.ToString() == "zINSTREAM")
                {
                    var len = new byte[4];
                    while (true)
                    {
                        await s.ReadExactlyAsync(len);
                        var n = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(len);
                        if (n == 0) break;
                        await s.ReadExactlyAsync(new byte[n]);
                    }
                    await s.WriteAsync(Encoding.ASCII.GetBytes(instreamReply + "\0"));
                }
                else
                {
                    await s.WriteAsync("ClamAV 1.4.1/27400\0"u8.ToArray());
                }
            }
        });
        return (listener, server);
    }

    private static ClamAvScanner Scanner(int port) =>
        new(Options.Create(new ClamAvOptions { Host = "127.0.0.1", Port = port, TimeoutSeconds = 10 }), NullLogger<ClamAvScanner>.Instance);

    [Fact]
    public async Task BUD018_ClamAv_stream_OK_is_clean_with_the_engine_version()
    {
        var (listener, server) = FakeClamd("stream: OK");
        using var _ = listener;
        var result = await Scanner(((IPEndPoint)listener.LocalEndpoint).Port).ScanAsync(new MemoryStream(PdfBytes), CancellationToken.None);
        await server;
        result.Verdict.Should().Be(ScanVerdict.Clean);
        result.Engine.Should().Be("ClamAV 1.4.1/27400");
    }

    [Fact]
    public async Task BUD018_ClamAv_Eicar_FOUND_is_infected()
    {
        var (listener, server) = FakeClamd("stream: Eicar-Signature FOUND");
        using var _ = listener;
        var result = await Scanner(((IPEndPoint)listener.LocalEndpoint).Port).ScanAsync(new MemoryStream(new byte[200_000]), CancellationToken.None);
        await server;
        result.Verdict.Should().Be(ScanVerdict.Infected);
        result.Detail.Should().Contain("FOUND");
    }

    [Fact]
    public async Task BUD018_ClamAv_unreachable_is_unavailable_not_clean()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        var result = await Scanner(port).ScanAsync(new MemoryStream(PdfBytes), CancellationToken.None);
        result.Verdict.Should().Be(ScanVerdict.Unavailable);
    }

    private static IAttachmentScanner Resolve(string environment, string? host)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Attachments:ClamAv:Host"] = host }).Build();
        var services = new ServiceCollection().AddLogging();
        services.AddAttachmentScanner(config, new FakeHostEnvironment(environment));
        return services.BuildServiceProvider().GetRequiredService<IAttachmentScanner>();
    }

    [Fact]
    public async Task BUD018_unconfigured_scanner_in_Production_refuses_every_upload()
    {
        var scanner = Resolve(Environments.Production, null);
        scanner.Should().BeOfType<UnavailableAttachmentScanner>();
        (await scanner.ScanAsync(new MemoryStream(PdfBytes), CancellationToken.None)).Verdict.Should().Be(ScanVerdict.Unavailable);
    }

    [Fact]
    public async Task Unconfigured_scanner_in_Development_records_NONE_Development()
    {
        var scanner = Resolve(Environments.Development, "");
        var r = await scanner.ScanAsync(new MemoryStream(PdfBytes), CancellationToken.None);
        r.Verdict.Should().Be(ScanVerdict.Clean);
        r.Engine.Should().Be("NONE (Development)");
    }

    [Fact]
    public async Task ScanEnabled_false_resolves_the_disabled_scanner_which_records_DISABLED()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Attachments:ScanEnabled"] = "false", ["Attachments:ClamAv:Host"] = "clamd.local",
        }).Build();
        var services = new ServiceCollection().AddLogging();
        services.AddAttachmentScanner(config, new FakeHostEnvironment(Environments.Production));
        var scanner = services.BuildServiceProvider().GetRequiredService<IAttachmentScanner>();
        scanner.Should().BeOfType<DisabledAttachmentScanner>();
        var r = await scanner.ScanAsync(new MemoryStream(PdfBytes), CancellationToken.None);
        r.Verdict.Should().Be(ScanVerdict.Clean);
        r.Engine.Should().Be("DISABLED");
    }
    [Fact]
    public void Configured_host_resolves_the_ClamAv_scanner_in_any_environment()
    {
        Resolve(Environments.Production, "clamd.local").Should().BeOfType<ClamAvScanner>();
    }

    [Fact]
    public async Task Storage_writes_under_the_storage_key_refuses_overwrite_and_path_escape()
    {
        var root = Path.Combine(Path.GetTempPath(), $"bbmb_att_{Guid.NewGuid():N}");
        try
        {
            var storage = new FileStorageService(Options.Create(new FileSettings
            {
                FolderPath = root, EmployeeFolderPath = "E", EmployeeFamilyFolderPath = "F", BudgetAttachmentFolderPath = "BudgetAttachments",
            }));
            var key = Guid.NewGuid().ToString();
            await storage.SaveFileAsync(new MemoryStream(PdfBytes), FileType.BudgetAttachment, key, CancellationToken.None);
            File.Exists(Path.Combine(root, "BudgetAttachments", key)).Should().BeTrue();
            await using (var read = storage.OpenRead(FileType.BudgetAttachment, key))
                read.Should().NotBeNull();
            await FluentActions.Awaiting(() => storage.SaveFileAsync(new MemoryStream(PdfBytes), FileType.BudgetAttachment, key, CancellationToken.None))
                .Should().ThrowAsync<IOException>();
            FluentActions.Invoking(() => storage.OpenRead(FileType.BudgetAttachment, "../x")).Should().Throw<ArgumentException>();
            storage.TryDelete(FileType.BudgetAttachment, key);
            storage.OpenRead(FileType.BudgetAttachment, key).Should().BeNull();
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private sealed class FakeHostEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;
        public string ApplicationName { get; set; } = "test";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
