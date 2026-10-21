using System.Security.Cryptography;

namespace BBMB.BudgetService.Shared.Services.Attachments
{
    /// <summary>
    /// Content detection and digest for T-22 (design §4.8 steps 1 to 3, DDD T-22, BUD-019). The content type
    /// is taken from the file's leading bytes, never from the browser header; a file whose bytes do not match
    /// its extension is refused (M-24).
    /// </summary>
    public static class AttachmentContent
    {
        /// <summary>T-22 <c>size_bytes</c> limit: "10 MB" of GEN-009 read as 10 × 1024 × 1024.</summary>
        public const long MaxSizeBytes = 10_485_760;

        /// <summary>Extensions accepted (DDD T-22 <c>file_extension</c> CHECK).</summary>
        public static readonly IReadOnlySet<string> AllowedExtensions =
            new HashSet<string>(StringComparer.Ordinal) { "pdf", "xml", "xls", "xlsx", "doc", "docx", "jpg", "jpeg", "png", "tif", "tiff" };

        /// <summary>Extensions an EVIDENCE attachment may have: pdf and the image formats (BUD-019).</summary>
        public static readonly IReadOnlySet<string> EvidenceExtensions =
            new HashSet<string>(StringComparer.Ordinal) { "pdf", "jpg", "jpeg", "png", "tif", "tiff" };

        private static readonly byte[] Pdf = "%PDF-"u8.ToArray();
        private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
        private static readonly byte[] TiffLe = [0x49, 0x49, 0x2A, 0x00];
        private static readonly byte[] TiffBe = [0x4D, 0x4D, 0x00, 0x2A];
        private static readonly byte[] Zip = [0x50, 0x4B, 0x03, 0x04];          // xlsx, docx (OOXML)
        private static readonly byte[] Ole = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1]; // xls, doc

        /// <summary>The lower-case extension without the dot, or empty.</summary>
        public static string ExtensionOf(string fileName) =>
            Path.GetExtension(fileName ?? string.Empty).TrimStart('.').ToLowerInvariant();

        /// <summary>
        /// The content type detected from <paramref name="head"/> when it agrees with <paramref name="extension"/>,
        /// else null (refused, M-24). XML has no fixed signature: it is accepted when the head, after an optional
        /// BOM and white space, begins with '&lt;'.
        /// </summary>
        public static string? DetectContentType(string extension, ReadOnlySpan<byte> head) => extension switch
        {
            "pdf" when head.StartsWith(Pdf) => "application/pdf",
            "png" when head.StartsWith(Png) => "image/png",
            "jpg" or "jpeg" when head.StartsWith(Jpeg) => "image/jpeg",
            "tif" or "tiff" when head.StartsWith(TiffLe) || head.StartsWith(TiffBe) => "image/tiff",
            "xlsx" when head.StartsWith(Zip) => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "docx" when head.StartsWith(Zip) => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xls" when head.StartsWith(Ole) => "application/vnd.ms-excel",
            "doc" when head.StartsWith(Ole) => "application/msword",
            "xml" when LooksLikeXml(head) => "application/xml",
            _ => null,
        };

        private static bool LooksLikeXml(ReadOnlySpan<byte> head)
        {
            if (head.StartsWith((ReadOnlySpan<byte>)[0xEF, 0xBB, 0xBF])) head = head[3..];
            foreach (var b in head)
            {
                if (b is (byte)' ' or (byte)'\t' or (byte)'\r' or (byte)'\n') continue;
                return b == (byte)'<';
            }
            return false;
        }

        /// <summary>Lower-case hex SHA-256 of the stream from its current position (T-22 <c>sha256_digest</c>).</summary>
        public static async Task<string> Sha256HexAsync(Stream content, CancellationToken ct)
        {
            var hash = await SHA256.HashDataAsync(content, ct);
            return Convert.ToHexStringLower(hash);
        }
    }
}
