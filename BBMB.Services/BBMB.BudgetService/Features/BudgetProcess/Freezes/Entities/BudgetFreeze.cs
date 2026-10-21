using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Freezes.Entities
{
    /// <summary>
    /// T-44 <c>budget.freeze</c> (DDD §5.7, F-20): the freeze of a cycle, append-only. One freeze per cycle, and
    /// no row can be removed, so the freeze cannot be lifted (BUD-193). Insertion is refused unless the
    /// FULL_BOARD allocation of the cycle exists, no T-37 row of the cycle carries redistribution_required, and
    /// every proposal of the cycle stands at S-15 (DB-19, M-30). On insertion the cycle's status becomes FROZEN.
    /// </summary>
    [Table("freeze")]
    public class BudgetFreeze : CreationStampedEntity
    {
        /// <summary>PK and FK T-14. One freeze per cycle.</summary>
        [Column("cycle_id")]
        public long CycleId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("fa_cao_approval_ref")]
        public string FaCaoApprovalRef { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        [Column("remarks")]
        public string Remarks { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("role_code")]
        public string RoleCode { get; set; } = string.Empty;

        [MaxLength(30)]
        [Column("on_behalf_of_post")]
        public string? OnBehalfOfPost { get; set; }
    }
}
