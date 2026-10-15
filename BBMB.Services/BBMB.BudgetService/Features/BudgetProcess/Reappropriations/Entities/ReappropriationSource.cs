using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities
{
    /// <summary>
    /// T-47 <c>budget.reappropriation_source</c> (DDD §5.8, URS Table 5 columns 7-10): a provision surrendered
    /// into a re-appropriation. Editable while the entry is OPEN only (DB-24). Not the destination (DB-24). The
    /// cumulative amount surrendered is not above the additional amount (DB-24, VR-08, M-18), and equals it on
    /// SUBMITTED. <c>is_removed</c> is a gated DDD deviation (§5.5 row c, CR-DDD-03): one-way false→true, OPEN
    /// entries only, so M-18 can invite the officer to remove a surrender line without a DELETE (DB-13 forbids
    /// delete).
    /// </summary>
    [Table("reappropriation_source")]
    public class ReappropriationSource : ActorStampedEntity
    {
        [Column("reappropriation_source_id")]
        public long ReappropriationSourceId { get; set; }

        /// <summary>FK T-46. Editable while OPEN only (DB-24).</summary>
        [Column("reappropriation_id")]
        public long ReappropriationId { get; set; }

        /// <summary>FK T-41. UK (reappropriation, provision). Not the destination (DB-24). Not restricted by eligibility (BUD-198).</summary>
        [Column("provision_id")]
        public long ProvisionId { get; set; }

        /// <summary>Column 10. CHECK &gt; 0. Cumulative not above column 5 (DB-24, VR-08, M-18).</summary>
        [Column("amount")]
        public decimal Amount { get; set; }

        /// <summary>§5.5 row c (CR-DDD-03): one-way false→true, OPEN entries only; a removed source is excluded from totals.</summary>
        [Column("is_removed")]
        public bool IsRemoved { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
