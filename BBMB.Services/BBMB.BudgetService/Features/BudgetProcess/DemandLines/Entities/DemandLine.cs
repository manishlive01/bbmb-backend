using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities
{
    /// <summary>
    /// T-30 <c>budget.demand_line</c>: one row of a demand (DDD T-30). Holds only what is entered on it; the
    /// displayed columns of URS Table 3/4 are joined by V-04. Every figure change writes a T-31 version by
    /// trigger (DB-14); figures change only while the state is open to the holder (DB-11).
    /// </summary>
    [Table("demand_line")]
    public class DemandLine : ActorStampedEntity
    {
        [Key]
        [Column("demand_line_id")]
        public long DemandLineId { get; set; }

        [Column("proposal_id")]
        public long ProposalId { get; set; }

        [Column("line_type")]
        [MaxLength(10)]
        public string LineType { get; set; } = string.Empty;

        /// <summary>Table 3 column 1, allotted by the module as max + 1 inside the transaction (BUD-079).</summary>
        [Column("serial_no")]
        public int SerialNo { get; set; }

        [Column("work_id")]
        public int? WorkId { get; set; }

        [Column("account_code_id")]
        public long? AccountCodeId { get; set; }

        [Column("unforeseen_name")]
        [MaxLength(500)]
        public string? UnforeseenName { get; set; }

        [Column("rbe_amount")]
        public decimal? RbeAmount { get; set; }

        [Column("obe_next_amount")]
        public decimal? ObeNextAmount { get; set; }

        [Column("justification_text")]
        [MaxLength(4000)]
        public string? JustificationText { get; set; }

        /// <summary>FK T-22, added in P2.</summary>
        [Column("justification_attachment_id")]
        public long? JustificationAttachmentId { get; set; }

        /// <summary>CE certificate of urgency (VR-03, BUD-091). FK T-22, added in P2.</summary>
        [Column("ce_certificate_attachment_id")]
        public long? CeCertificateAttachmentId { get; set; }

        /// <summary>CE approval where VR-05 is crossed (BUD-278). FK T-22, added in P2.</summary>
        [Column("ce_amr_approval_attachment_id")]
        public long? CeAmrApprovalAttachmentId { get; set; }

        /// <summary>The base of a percentage check was nil or negative (M-32, BUD-243).</summary>
        [Column("pct_check_not_applied")]
        public bool PctCheckNotApplied { get; set; }

        /// <summary>VR-15 could not be applied: expenditure not available (BUD-317).</summary>
        [Column("floor_check_unchecked")]
        public bool FloorCheckUnchecked { get; set; }

        /// <summary>Added while the proposal stood returned (BUD-320).</summary>
        [Column("added_on_return")]
        public bool AddedOnReturn { get; set; }

        /// <summary>One-way false → true; the row is never removed (DB-13, BUD-319).</summary>
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }

        [Column("deleted_by")]
        public Guid? DeletedBy { get; set; }

        /// <summary>FK T-34, added in P2.</summary>
        [Column("upload_batch_id")]
        public long? UploadBatchId { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
