using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.Entities
{
    /// <summary>
    /// T-38 <c>budget.recommendation_version</c>: every value a recommendation has held, append-only, written by
    /// trigger <c>budget.fn_capture_recommendation_version</c> only (DB-14). <c>minutes_allocation_id</c> is
    /// added in P4.
    /// </summary>
    [Table("recommendation_version")]
    public class RecommendationVersion : CreationStampedEntity
    {
        [Column("recommendation_id")]
        public long RecommendationId { get; private set; }

        [Column("version_no")]
        public short VersionNo { get; private set; }

        [Column("rbe_recommended")]
        public decimal? RbeRecommended { get; private set; }

        [Column("obe_next_recommended")]
        public decimal? ObeNextRecommended { get; private set; }

        [Column("concurrence_ref")]
        public string? ConcurrenceRef { get; private set; }

        /// <summary>FK T-40 (P4): the minutes allocation the recommendation gave effect to at this version (DB-17).</summary>
        [Column("minutes_allocation_id")]
        public long? MinutesAllocationId { get; private set; }
    }
}
