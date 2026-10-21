using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities
{
    /// <summary>
    /// T-22 <c>budget.attachment</c>: every uploaded file, append-only (DDD T-22, design §4.8). The row is
    /// written only after the file has been stored and scanned clean; a failed scan stores no row and no file.
    /// Owning records refer to it through a column of their own, so a mandatory file is a NOT NULL FK and the
    /// evidence class is checked by DB-08 (VR-10).
    /// </summary>
    [Table("attachment")]
    public class BudgetAttachment : CreationStampedEntity
    {
        [Key]
        [Column("attachment_id")]
        public long AttachmentId { get; set; }
        /// <summary>File name on the file server; never reused (UK).</summary>
        [Column("storage_key")]
        public Guid StorageKey { get; set; }
        [Column("original_file_name")]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;
        /// <summary>pdf, xml, xls, xlsx, doc, docx, jpg, jpeg, png, tif, tiff (CHECK).</summary>
        [Column("file_extension")]
        [MaxLength(5)]
        public string FileExtension { get; set; } = string.Empty;
        /// <summary>As detected on upload from the content, not as declared by the browser.</summary>
        [Column("content_type")]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;
        /// <summary><see cref="AttachmentEvidenceClass"/>; EVIDENCE admits only pdf and the image formats (CHECK).</summary>
        [Column("evidence_class")]
        [MaxLength(10)]
        public string EvidenceClass { get; set; } = string.Empty;
        /// <summary>CHECK &gt; 0 and ≤ 10 485 760 (GEN-009 "10 MB" read as 10 × 1024 × 1024).</summary>
        [Column("size_bytes")]
        public long SizeBytes { get; set; }
        /// <summary>Lower-case hex SHA-256 computed on the server (CHECK ^[0-9a-f]{64}$, BUD-019).</summary>
        [Column("sha256_digest")]
        [MaxLength(64)]
        public string Sha256Digest { get; set; } = string.Empty;
        /// <summary>Name and signature version of the scanner (BUD-018).</summary>
        [Column("scan_engine")]
        [MaxLength(100)]
        public string ScanEngine { get; set; } = string.Empty;
        [Column("scanned_at")]
        public DateTimeOffset ScannedAt { get; set; }
    }

    /// <summary>T-22 <c>evidence_class</c> values (CHECK).</summary>
    public static class AttachmentEvidenceClass
    {
        public const string Evidence = "EVIDENCE";
        public const string Supporting = "SUPPORTING";
    }
}
