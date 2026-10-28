using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities
{
    /// <summary>
    /// <c>budget.budget_role_right</c> (requirement 1): a role-to-right assignment (formerly the arrays inside
    /// each <c>BudgetRights.Rights</c> entry). Unique per (role, right); both FKs RESTRICT.
    /// </summary>
    [Table("budget_role_right")]
    public class BudgetRoleRight : ActorStampedEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("role_id")]
        public long RoleId { get; set; }

        [Column("right_id")]
        public long RightId { get; set; }

        /// <summary>
        /// Soft-delete flag for the grant. Revoking a right deactivates the row (an audited UPDATE) rather
        /// than deleting it — the config tables are append-only and carry a DELETE-forbid trigger, and the
        /// snapshot reads active rows only (requirement 1).
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(RoleId))]
        public BudgetRole? Role { get; set; }

        [ForeignKey(nameof(RightId))]
        public BudgetRight? Right { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
