using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Entities
{
    /// <summary>
    /// T-43 <c>budget.estimate_allocation</c> (DDD §5.7, F-29): a WORK provision spread across the estimate codes
    /// of the work's sanctions. One row per (provision, sanction). Total per provision not above the provision
    /// available (DB-23, VR-12, M-28). Mutable ([A]); every change audited (BUD-290).
    /// </summary>
    [Table("estimate_allocation")]
    public class EstimateAllocation : ActorStampedEntity
    {
        [Column("estimate_allocation_id")]
        public long EstimateAllocationId { get; set; }

        /// <summary>FK T-41. A WORK provision (DB-04).</summary>
        [Column("provision_id")]
        public long ProvisionId { get; set; }

        /// <summary>FK T-27. A sanction of the same work carrying an estimate code (DB-04). UK (provision, sanction).</summary>
        [Column("technical_sanction_id")]
        public long TechnicalSanctionId { get; set; }

        /// <summary>CHECK ≥ 0. Total per provision not above the provision available (DB-23, VR-12, M-28).</summary>
        [Column("amount")]
        public decimal Amount { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
