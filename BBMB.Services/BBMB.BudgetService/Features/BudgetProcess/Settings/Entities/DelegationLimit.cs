using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Entities
{
    /// <summary>
    /// T-19 <c>budget.delegation_limit</c>: powers of the competent authorities (R-06), append-only
    /// (DDD T-19, DB-27). A level with no row in force carries no limit, as a null <c>max_amount</c> does.
    /// Read by the BUD-055 delegation warning (M-27), which warns and does not refuse.
    /// </summary>
    [Table("delegation_limit")]
    public class DelegationLimit : CreationStampedEntity
    {
        [Key]
        [Column("delegation_limit_id")]
        public long DelegationLimitId { get; set; }
        /// <summary><see cref="CompetentAuthority"/>: SECRETARY, CE, SE or EE (AAP-001).</summary>
        [Column("authority_level")]
        [MaxLength(10)]
        public string AuthorityLevel { get; set; } = string.Empty;
        /// <summary>Whole rupees; null means no limit; CHECK &gt; 0 where set.</summary>
        [Column("max_amount")]
        public decimal? MaxAmount { get; set; }
        [Column("effective_from")]
        public DateOnly EffectiveFrom { get; set; }
        /// <summary>The order of delegation (R-06).</summary>
        [Column("source_ref")]
        [MaxLength(100)]
        public string SourceRef { get; set; } = string.Empty;
    }

    /// <summary>The four levels of AAP-001 (T-19 <c>authority_level</c>, T-26 <c>competent_authority</c>).</summary>
    public static class CompetentAuthority
    {
        public const string Secretary = "SECRETARY";
        public const string Ce = "CE";
        public const string Se = "SE";
        public const string Ee = "EE";
        public static readonly IReadOnlyList<string> All = [Secretary, Ce, Se, Ee];
    }
}
