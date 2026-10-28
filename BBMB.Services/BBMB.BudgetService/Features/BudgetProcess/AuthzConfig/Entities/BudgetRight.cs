using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities
{
    /// <summary>
    /// <c>budget.budget_right</c> (requirement 1): an available right key, e.g. <c>F01.Create</c>
    /// (formerly the keys of <c>BudgetRights.Rights</c>). Role-to-right assignment lives in
    /// <see cref="BudgetRoleRight"/>.
    /// </summary>
    [Table("budget_right")]
    public class BudgetRight : ActorStampedEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("right_key")]
        [MaxLength(100)]
        public string RightKey { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public uint Version { get; set; }
    }
}
