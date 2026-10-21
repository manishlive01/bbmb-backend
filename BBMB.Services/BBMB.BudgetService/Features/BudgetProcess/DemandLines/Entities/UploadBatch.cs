using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities
{
    /// <summary>
    /// T-34 <c>budget.upload_batch</c>: a committed spreadsheet upload of works demand (DDD T-34, F-08,
    /// BUD-101). Append-only (<c>fn_forbid_change</c>): one row is written when an upload is accepted, inside
    /// the one transaction that also writes the T-22 supporting attachment and the T-30 lines; a rejected file
    /// writes nothing and leaves no row (BUD-100). The uploading officer and time are the [C] columns, filled by
    /// <c>budget.fn_require_actor</c> (DB-28); the application never writes them.
    /// </summary>
    [Table("upload_batch")]
    public class UploadBatch : CreationStampedEntity
    {
        [Key]
        [Column("upload_batch_id")]
        public long UploadBatchId { get; set; }

        /// <summary>FK T-29. The WORKS proposal in S-01 the lines were added to (DB-11, BUD-102).</summary>
        [Column("proposal_id")]
        public long ProposalId { get; set; }

        /// <summary>FK T-22. The file as uploaded, held in the supporting evidence class.</summary>
        [Column("file_attachment_id")]
        public long FileAttachmentId { get; set; }

        /// <summary>CHECK &gt; 0: an accepted upload commits at least one line (DDD T-34).</summary>
        [Column("lines_committed")]
        public int LinesCommitted { get; set; }
    }
}
