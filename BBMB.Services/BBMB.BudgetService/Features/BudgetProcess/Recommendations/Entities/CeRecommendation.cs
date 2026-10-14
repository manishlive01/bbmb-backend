using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.Entities
{
    /// <summary>
    /// T-37 <c>budget.ce_recommendation</c>: columns 9 and 10 for a Chief Engineer, per head-wise category and
    /// account code (DDD T-37, BUD-302). Every change writes a T-38 version by trigger (DB-14).
    /// <c>minutes_allocation_id</c> (FK T-40, DB-17) is added in P4.
    /// </summary>
    [Table("ce_recommendation")]
    public class CeRecommendation : ActorStampedEntity
    {
        [Key]
        [Column("recommendation_id")]
        public long RecommendationId { get; set; }

        [Column("cycle_id")]
        public long CycleId { get; set; }

        /// <summary>DNA, RECEIPTS or CAPEX.</summary>
        [Column("category")]
        [MaxLength(8)]
        public string Category { get; set; } = string.Empty;

        /// <summary>A CE office (DB-01, BUD-303).</summary>
        [Column("ce_office_id")]
        public long CeOfficeId { get; set; }

        [Column("account_code_id")]
        public long AccountCodeId { get; set; }

        [Column("rbe_recommended")]
        public decimal? RbeRecommended { get; set; }

        [Column("obe_next_recommended")]
        public decimal? ObeNextRecommended { get; set; }

        /// <summary>Required where either figure is set (BUD-153, BUD-154).</summary>
        [Column("concurrence_ref")]
        [MaxLength(100)]
        public string? ConcurrenceRef { get; set; }

        /// <summary>BUD-279 flag; set by trigger (DB-18, P3).</summary>
        [Column("redistribution_required")]
        public bool RedistributionRequired { get; set; }

        [Column("distribution_confirmed_at")]
        public DateTimeOffset? DistributionConfirmedAt { get; set; }

        [Column("distribution_confirmed_by")]
        public Guid? DistributionConfirmedBy { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
