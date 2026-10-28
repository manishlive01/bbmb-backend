using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities
{
    /// <summary>
    /// <c>budget.budget_project_settings</c> (requirement 1): the Budget project's identity and the settings
    /// that were formerly in <c>appsettings.json</c> under <c>BudgetProcess</c> / <c>BudgetRights.ProjectName</c>.
    /// Exactly one active row frames authorization and charge resolution; <see cref="EmployeeChargeProjectCode"/>
    /// is the <c>employee_charge.project_code</c> of the Budget project (formerly <c>BudgetProcess:ProjectCode</c>,
    /// a seeded editable value — never hard-coded in C#). Mutable and audited: a change is an audited UPDATE,
    /// its history is in <c>budget.audit_log</c>.
    /// </summary>
    [Table("budget_project_settings")]
    public class BudgetProjectSettings : ActorStampedEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>The token's <c>ProjectId</c> for the Budget project. NULL until confirmed (OQ-2); name match is the interim.</summary>
        [Column("sso_project_id")]
        public Guid? SsoProjectId { get; set; }

        /// <summary>The token's <c>ProjectName</c> for the Budget project (formerly <c>BudgetRights.ProjectName</c>).</summary>
        [Column("sso_project_name")]
        [MaxLength(200)]
        public string SsoProjectName { get; set; } = string.Empty;

        /// <summary><c>employee_charge.project_code</c> of the Budget project (formerly <c>BudgetProcess:ProjectCode</c>).</summary>
        [Column("employee_charge_project_code")]
        [MaxLength(50)]
        public string EmployeeChargeProjectCode { get; set; } = string.Empty;

        /// <summary>Edit-lease length in minutes (formerly <c>BudgetProcess:EditLeaseMinutes</c>).</summary>
        [Column("edit_lease_minutes")]
        public int EditLeaseMinutes { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>xmin optimistic concurrency token.</summary>
        [Timestamp]
        public uint Version { get; set; }
    }
}
