using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Shared.Services.Attachments
{
    /// <summary>Configuration <c>Attachments:ClamAv</c> (design §4.8). Host unset means no scanner.</summary>
    public sealed class ClamAvOptions
    {
        public const string Section = "Attachments:ClamAv";
        public string? Host { get; set; }
        public int Port { get; set; } = 3310;
        public int TimeoutSeconds { get; set; } = 60;
    }

    /// <summary>
    /// Speaks the clamd <c>INSTREAM</c> protocol over TCP (no package, design §4.8): <c>zINSTREAM\0</c>, then
    /// chunks each prefixed by a 4-byte big-endian length, then a zero-length chunk; the reply is
    /// <c>stream: OK</c> or <c>stream: &lt;signature&gt; FOUND</c>. The engine version is taken from
    /// <c>zVERSION\0</c>. Any connection or protocol failure is <see cref="ScanVerdict.Unavailable"/>, so the
    /// upload fails closed.
    /// </summary>
    public sealed class ClamAvScanner(IOptions<ClamAvOptions> options, ILogger<ClamAvScanner> logger) : IAttachmentScanner
    {
        private const int ChunkSize = 64 * 1024;
        private readonly ClamAvOptions _o = options.Value;

        public async Task<ScanResult> ScanAsync(Stream content, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_o.Host))
                return new ScanResult(ScanVerdict.Unavailable, "ClamAV", "not configured");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(_o.TimeoutSeconds));
            try
            {
                var version = (await CommandAsync("zVERSION\0", null, timeout.Token)).Trim();
                var reply = (await CommandAsync("zINSTREAM\0", content, timeout.Token)).Trim();
                var engine = Truncate(string.IsNullOrEmpty(version) ? "ClamAV" : version, 100);
                if (reply.EndsWith("OK", StringComparison.Ordinal))
                    return new ScanResult(ScanVerdict.Clean, engine);
                if (reply.EndsWith("FOUND", StringComparison.Ordinal))
                    return new ScanResult(ScanVerdict.Infected, engine, reply);
                return new ScanResult(ScanVerdict.Unavailable, engine, reply);
            }
            catch (Exception ex) when (ex is SocketException or IOException or OperationCanceledException && !ct.IsCancellationRequested)
            {
                logger.LogError(ex, "ClamAV scanner at {Host}:{Port} unavailable", _o.Host, _o.Port);
                return new ScanResult(ScanVerdict.Unavailable, "ClamAV", ex.GetType().Name);
            }
        }

        private async Task<string> CommandAsync(string command, Stream? payload, CancellationToken ct)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_o.Host!, _o.Port, ct);
            await using var net = client.GetStream();
            await net.WriteAsync(Encoding.ASCII.GetBytes(command), ct);
            if (payload is not null)
            {
                var buffer = new byte[ChunkSize];
                var length = new byte[4];
                int read;
                while ((read = await payload.ReadAsync(buffer, ct)) > 0)
                {
                    System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(length, read);
                    await net.WriteAsync(length, ct);
                    await net.WriteAsync(buffer.AsMemory(0, read), ct);
                }
                System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(length, 0);
                await net.WriteAsync(length, ct);
            }
            await net.FlushAsync(ct);
            using var reader = new StreamReader(net, Encoding.ASCII);
            var reply = await reader.ReadToEndAsync(ct);
            return reply.TrimEnd('\0', '\n', '\r');
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];
    }

    /// <summary>
    /// Development only (design §4.8): records <c>scan_engine = 'NONE (Development)'</c> and scans nothing.
    /// Never registered outside the Development environment.
    /// </summary>
    public sealed class NoScanDevScanner : IAttachmentScanner
    {
        public const string Engine = "NONE (Development)";
        public Task<ScanResult> ScanAsync(Stream content, CancellationToken ct) =>
            Task.FromResult(new ScanResult(ScanVerdict.Clean, Engine));
    }

    /// <summary>Outside Development with no scanner configured: refuses every upload (503 path, fail closed).</summary>
    public sealed class UnavailableAttachmentScanner : IAttachmentScanner
    {
        public Task<ScanResult> ScanAsync(Stream content, CancellationToken ct) =>
            Task.FromResult(new ScanResult(ScanVerdict.Unavailable, "none", "no scanner configured"));
    }

    public static class AttachmentScannerRegistration
    {
        /// <summary>
        /// ClamAV when <c>Attachments:ClamAv:Host</c> is set; else, in Development only, the no-scan scanner;
        /// else a scanner that refuses every upload (design §4.8, OI-12).
        /// </summary>
        public static IServiceCollection AddAttachmentScanner(this IServiceCollection services, IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.Configure<ClamAvOptions>(configuration.GetSection(ClamAvOptions.Section));
            var host = configuration[$"{ClamAvOptions.Section}:Host"];
            if (!string.IsNullOrWhiteSpace(host))
                services.AddSingleton<IAttachmentScanner, ClamAvScanner>();
            else if (environment.IsDevelopment())
                services.AddSingleton<IAttachmentScanner, NoScanDevScanner>();
            else
                services.AddSingleton<IAttachmentScanner, UnavailableAttachmentScanner>();
            return services;
        }
    }
}
