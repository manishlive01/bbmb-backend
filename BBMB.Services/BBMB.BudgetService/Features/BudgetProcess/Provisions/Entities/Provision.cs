using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities
{
    /// <summary>
    /// T-41 <c>budget.provision</c> (DDD §5.7): activated provision, written once. One row per holder, financial
    /// year and basis activated. Nothing in the row changes after it is written except the supersession marker
    /// (DB-05); the amount available is the sum of its T-42 movements (V-02). A demand line is activated if and
    /// only if a provision refers to it (BUD-183). The operative provision of a year is the RBE provision where
    /// one is live and the OBE provision otherwise (V-02).
    /// </summary>
    [Table("provision")]
    public class Provision : CreationStampedEntity
    {
        [Column("provision_id")]
        public long ProvisionId { get; set; }

        /// <summary>FK master.fa_financial_years. The year the money is for.</summary>
        [Column("fin_year")]
        public int FinYear { get; set; }

        /// <summary>CHECK in RBE, OBE.</summary>
        [Required]
        [MaxLength(3)]
        [Column("basis")]
        public string Basis { get; set; } = string.Empty;

        /// <summary>CHECK as T-29 (WORKS, DNA, RECEIPTS, CAPEX).</summary>
        [Required]
        [MaxLength(8)]
        [Column("category")]
        public string Category { get; set; } = string.Empty;

        /// <summary>CHECK in WORK, HEAD, UNFORESEEN.</summary>
        [Required]
        [MaxLength(10)]
        [Column("holder_type")]
        public string HolderType { get; set; } = string.Empty;

        /// <summary>FK T-23. Not null exactly for WORK (CHECK).</summary>
        [Column("work_id")]
        public int? WorkId { get; set; }

        /// <summary>FK T-13. For WORK the Division of the work (DB-04); for HEAD and UNFORESEEN the holder office.</summary>
        [Column("office_id")]
        public long OfficeId { get; set; }

        /// <summary>FK T-07. Not null for WORK and HEAD; for UNFORESEEN the head in T-17 at allocation, or null (BUD-277).</summary>
        [Column("account_code_id")]
        public long? AccountCodeId { get; set; }

        /// <summary>FK T-30. The line activated, for WORK and UNFORESEEN from an allocation.</summary>
        [Column("demand_line_id")]
        public long? DemandLineId { get; set; }

        /// <summary>FK T-37. For HEAD from an allocation: the recommendation.</summary>
        [Column("recommendation_id")]
        public long? RecommendationId { get; set; }

        /// <summary>FK T-39. The distributed share, where distributed.</summary>
        [Column("distribution_id")]
        public long? DistributionId { get; set; }

        /// <summary>FK T-40. Null for a provision opened by re-appropriation to a new work or by an unforeseen authorisation.</summary>
        [Column("allocation_id")]
        public long? AllocationId { get; set; }

        /// <summary>For OBE, 1 April of fin_year (BUD-182, CHECK).</summary>
        [Column("effective_from")]
        public DateOnly EffectiveFrom { get; set; }

        /// <summary>False exactly for RECEIPTS (CHECK, BUD-307).</summary>
        [Column("budgetary_check")]
        public bool BudgetaryCheck { get; set; }

        /// <summary>FK T-41. Set once (BUD-285). The only column that changes (DB-05).</summary>
        [Column("superseded_by_provision_id")]
        public long? SupersededByProvisionId { get; set; }
    }

    /// <summary>T-41 <c>basis</c> values (CHECK).</summary>
    public static class ProvisionBasis
    {
        public const string Rbe = "RBE";
        public const string Obe = "OBE";
    }

    /// <summary>T-41 <c>holder_type</c> values (CHECK).</summary>
    public static class ProvisionHolderType
    {
        public const string Work = "WORK";
        public const string Head = "HEAD";
        public const string Unforeseen = "UNFORESEEN";
    }
}
