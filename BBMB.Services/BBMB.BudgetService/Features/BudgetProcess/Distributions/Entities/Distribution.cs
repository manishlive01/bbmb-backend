using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Entities
{
    /// <summary>
    /// T-39 <c>budget.distribution</c>: the Chief Engineer's distribution of a recommended head to the Circles
    /// and Divisions of the charge (DDD T-39, F-14, DNA-011, BUD-155 to BUD-160). One row per
    /// (recommendation, office). The per-recommendation totals stay within the recommended figure, column by
    /// column, checked at commit (DB-18, VR-07, M-14). The balance retained at the CE office is computed, not
    /// stored (V-05, BUD-158, BUD-160). The row is mutable (the CE revises a share); the [A] actor columns are
    /// trigger-filled.
    /// </summary>
    [Table("distribution")]
    public class Distribution : ActorStampedEntity
    {
        [Key]
        [Column("distribution_id")]
        public long DistributionId { get; set; }

        /// <summary>FK T-37. The recommendation this share is distributed from.</summary>
        [Column("recommendation_id")]
        public long RecommendationId { get; set; }

        /// <summary>FK T-13. A Circle or Division within the charge of the CE (DB-01, BUD-156). UK (recommendation, office).</summary>
        [Column("office_id")]
        public long OfficeId { get; set; }

        /// <summary>Column 9 as distributed, whole rupees, default 0.</summary>
        [Column("rbe_amount")]
        public decimal RbeAmount { get; set; }

        /// <summary>Column 10 as distributed, whole rupees, default 0.</summary>
        [Column("obe_next_amount")]
        public decimal ObeNextAmount { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
