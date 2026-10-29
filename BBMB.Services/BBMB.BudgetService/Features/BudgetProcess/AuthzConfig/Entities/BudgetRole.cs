using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities
{
    /// <summary>
    /// <c>budget.budget_role</c> (requirement 1): a Budget role and the T-13 office level it acts from
    /// (formerly <c>BudgetRights.RoleOfficeLevels</c>). <see cref="IsAdmin"/> marks a role eligible to be a
    /// Budget administrator; it is only an <i>effective</i> admin once it has an APPROVED, document-backed
    /// <see cref="BudgetAdminRoleAssignment"/> (requirement 4).
    /// </summary>
    [Table("budget_role")]
    public class BudgetRole : ActorStampedEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("role_name")]
        [MaxLength(200)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>One of <see cref="BBMB.BudgetService.Features.BudgetProcess.Offices.Entities.OfficeLevel"/>.</summary>
        [Column("office_level")]
        [MaxLength(12)]
        public string OfficeLevel { get; set; } = string.Empty;

        [Column("is_admin")]
        public bool IsAdmin { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public uint Version { get; set; }
    }
}
