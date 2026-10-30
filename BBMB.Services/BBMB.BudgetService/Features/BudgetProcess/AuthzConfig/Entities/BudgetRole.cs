using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.Entites;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities
{
    /// <summary>
    /// <c>budget.budget_role</c> (requirement 1): a Budget role and the office level it acts from.
    /// <para>
    /// Two owners write this row and never each other's columns:
    /// <list type="bullet">
    /// <item><b>Auth SSO sync</b> owns <see cref="SsoRoleId"/>, <see cref="RoleName"/>, <see cref="SsoIsActive"/> and
    /// <see cref="LastSyncedAt"/> (<c>POST /api/budget-config/roles/sync</c>);</item>
    /// <item><b>the Budget administrator</b> owns <see cref="OfficeLevelId"/>, <see cref="IsAdmin"/>,
    /// <see cref="IsActive"/> and the role's rights and query fields (<c>roles/configure</c>).</item>
    /// </list>
    /// A role is authorised only when it is configured (see <see cref="BudgetRoleLevel.IsAuthorisable"/>): active,
    /// SSO-active, with a valid office level code (<see cref="OfficeLevelCode"/>) and at least one active right.
    /// </para>
    /// <see cref="IsAdmin"/> marks a role eligible to be a Budget administrator; it is only an <i>effective</i>
    /// admin once it has an APPROVED, document-backed <see cref="BudgetAdminRoleAssignment"/> (requirement 4).
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
        /// <summary>
        /// FK to the <c>levels</c> master (ON DELETE RESTRICT), for display and reference only. Authorisation does
        /// not read it and the level's name is never interpreted.
        /// </summary>
        [Column("office_level_id")]
        public int? OfficeLevelId { get; set; }
        [ForeignKey(nameof(OfficeLevelId))]
        public Levels? OfficeLevel { get; set; }

        /// <summary>
        /// <c>office_level</c>: the role's Budget office level type, one of <c>OfficeLevel.All</c> (CHECK
        /// <c>ck_budget_role_office_level</c>). This is the permanent source of the type. NULL = unconfigured, so the
        /// role is denied (<see cref="BudgetRoleLevel.IsAuthorisable"/>).
        /// </summary>
        [Column("office_level")]
        [MaxLength(12)]
        public string? OfficeLevelCode { get; set; }

        /// <summary>Sets the office level reference and the office level type together.</summary>
        public void SetOfficeLevel(int? levelId, string? officeLevelCode)
        {
            OfficeLevelId = levelId;
            OfficeLevelCode = officeLevelCode;
        }
        /// <summary>The Auth SSO role id. NULL only for a legacy seeded row not yet matched by a sync.</summary>
        [Column("sso_role_id")]
        public Guid? SsoRoleId { get; set; }
        /// <summary>Active status as last reported by Auth SSO. False ⇒ the role is denied.</summary>
        [Column("sso_is_active")]
        public bool SsoIsActive { get; set; } = true;
        /// <summary>When the last sync saw (or deactivated) this role. NULL for a role never synchronised.</summary>
        [Column("last_synced_at")]
        public DateTimeOffset? LastSyncedAt { get; set; }
        [Column("is_admin")]
        public bool IsAdmin { get; set; }
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        [Timestamp]
        public uint Version { get; set; }
    }
}
