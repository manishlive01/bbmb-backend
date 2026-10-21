using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Entities
{
    /// <summary>
    /// T-40 <c>budget.allocation</c> (DDD §5.7): an allocation made on the authority of recorded minutes,
    /// append-only (BUD-183). One row per (cycle, body). BSC activates the revised estimate of the current
    /// year; FULL_BOARD the original estimate of the next year and the revised estimate superseding the BSC
    /// provision. The minutes attachment is evidence class (VR-10, M-16, DB-08).
    /// </summary>
    [Table("allocation")]
    public class Allocation : CreationStampedEntity
    {
        [Column("allocation_id")]
        public long AllocationId { get; set; }

        /// <summary>FK T-14. UK (cycle, body).</summary>
        [Column("cycle_id")]
        public long CycleId { get; set; }

        /// <summary>CHECK in BSC, FULL_BOARD. UK (cycle, body).</summary>
        [Required]
        [MaxLength(10)]
        [Column("body")]
        public string Body { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("minutes_reference")]
        public string MinutesReference { get; set; } = string.Empty;

        [Column("minutes_date")]
        public DateOnly MinutesDate { get; set; }

        /// <summary>FK T-22. Evidence class (VR-10, BUD-180, M-16, DB-08).</summary>
        [Column("minutes_attachment_id")]
        public long MinutesAttachmentId { get; set; }

        /// <summary>The e-Office reference of the FA &amp; CAO's approval (BUD-188).</summary>
        [Required]
        [MaxLength(100)]
        [Column("fa_cao_approval_ref")]
        public string FaCaoApprovalRef { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        [Column("remarks")]
        public string Remarks { get; set; } = string.Empty;

        /// <summary>Shown before confirmation (BUD-189); equal to the provisions created (DB-20).</summary>
        [Column("lines_activated")]
        public int LinesActivated { get; set; }

        /// <summary>CHECK ≥ 0. Equal to the sum of the allocation movements (DB-20).</summary>
        [Column("total_activated")]
        public decimal TotalActivated { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("role_code")]
        public string RoleCode { get; set; } = string.Empty;

        [MaxLength(30)]
        [Column("on_behalf_of_post")]
        public string? OnBehalfOfPost { get; set; }
    }

    /// <summary>T-40 <c>body</c> values (CHECK).</summary>
    public static class AllocationBody
    {
        public const string Bsc = "BSC";
        public const string FullBoard = "FULL_BOARD";
    }
}
