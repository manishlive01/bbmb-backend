using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities
{
    /// <summary>
    /// T-46 <c>budget.reappropriation</c> (DDD §5.8, URS Table 5, F-21/F-22): one re-appropriation entry. The
    /// only in-year change to provision after the freeze (SRS §6; ALC-006, BUD-191). Exactly one destination
    /// (an existing provision, or a new work without a code or provision, REA-011). Status moves from OPEN once
    /// (DB-24). One OPEN entry per officer at a time (partial UK on created_by where OPEN, BUD-201). On SUBMITTED
    /// the destination provision is opened for a new work and its code generated, a REAPPROP_IN movement for the
    /// destination and a REAPPROP_OUT for each source are written, and DB-21 checks they balance.
    /// </summary>
    [Table("reappropriation")]
    public class Reappropriation : ActorStampedEntity
    {
        [Column("reappropriation_id")]
        public long ReappropriationId { get; set; }

        /// <summary>FK master.fa_financial_years.</summary>
        [Column("fin_year")]
        public int FinYear { get; set; }

        /// <summary>Column 1. UK (fin_year, serial).</summary>
        [Column("serial_no")]
        public int SerialNo { get; set; }

        /// <summary>CHECK in OPEN, SUBMITTED, ABANDONED. Moves from OPEN only, once (DB-24).</summary>
        [Required]
        [MaxLength(9)]
        [Column("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>FK T-41. Columns 2 to 4. Not null where SUBMITTED (CHECK).</summary>
        [Column("destination_provision_id")]
        public long? DestinationProvisionId { get; set; }

        /// <summary>FK T-23. A work without a code (REA-011, BUD-282/283). One of the two destinations while OPEN (CHECK).</summary>
        [Column("destination_new_work_id")]
        public int? DestinationNewWorkId { get; set; }

        /// <summary>Column 5. CHECK &gt; 0.</summary>
        [Column("additional_amount")]
        public decimal AdditionalAmount { get; set; }

        /// <summary>Column 12 (BUD-205).</summary>
        [Required]
        [MaxLength(100)]
        [Column("eoffice_order_ref")]
        public string EofficeOrderRef { get; set; } = string.Empty;

        /// <summary>Column 12.</summary>
        [Required]
        [MaxLength(4000)]
        [Column("remarks")]
        public string Remarks { get; set; } = string.Empty;

        /// <summary>FK T-22. Column 13. Evidence class. Not null where SUBMITTED (CHECK, VR-10, BUD-202).</summary>
        [Column("order_attachment_id")]
        public long? OrderAttachmentId { get; set; }

        [Column("closed_at")]
        public DateTimeOffset? ClosedAt { get; set; }

        [Column("closed_by")]
        public Guid? ClosedBy { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }

    /// <summary>T-46 <c>status</c> values (CHECK).</summary>
    public static class ReappropriationStatus
    {
        public const string Open = "OPEN";
        public const string Submitted = "SUBMITTED";
        public const string Abandoned = "ABANDONED";
    }
}
