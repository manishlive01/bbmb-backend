using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.Entities
{
    /// <summary>
    /// T-45 <c>budget.correction_flag</c>: a basis of allocation that has changed (DDD T-45, BUD-065, BUD-269,
    /// BUD-309). Created in P2 because F-04 revisions raise AA_REDUCED and AA_HEAD_CHANGED (design §5.3).
    /// <c>provision_id</c> (FK T-41) is added in P4 and <c>cleared_by_reappropriation_id</c> (FK T-46) in P5;
    /// until P4 a flag names its work (CHECK). The clearance is set once (DB-05).
    /// </summary>
    [Table("correction_flag")]
    public class CorrectionFlag : CreationStampedEntity
    {
        [Key]
        [Column("correction_flag_id")]
        public long CorrectionFlagId { get; set; }
        /// <summary><see cref="CorrectionFlagReason"/> (CHECK).</summary>
        [Column("reason")]
        [MaxLength(20)]
        public string Reason { get; set; } = string.Empty;
        [Column("work_id")]
        public int? WorkId { get; set; }
        /// <summary>Shown with M-34.</summary>
        [Column("figure_allocated")]
        public decimal? FigureAllocated { get; set; }
        [Column("figure_supported")]
        public decimal? FigureSupported { get; set; }
        /// <summary>True where the basis is below expenditure booked; blocks F-29 for the work (BUD-309).</summary>
        [Column("is_critical")]
        public bool IsCritical { get; set; }
        /// <summary>Set once (DB-05).</summary>
        [Column("cleared_at")]
        public DateTimeOffset? ClearedAt { get; set; }
        [Column("cleared_by")]
        public Guid? ClearedBy { get; set; }
    }

    /// <summary>T-45 <c>reason</c> values (CHECK).</summary>
    public static class CorrectionFlagReason
    {
        public const string AaReduced = "AA_REDUCED";
        public const string AaHeadChanged = "AA_HEAD_CHANGED";
        public const string ProposalReopened = "PROPOSAL_REOPENED";
        public const string ReallocateEstimates = "REALLOCATE_ESTIMATES";
        public const string BelowExpenditure = "BELOW_EXPENDITURE";
    }
}
