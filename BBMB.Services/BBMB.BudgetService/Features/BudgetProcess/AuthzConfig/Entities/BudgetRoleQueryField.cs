using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities
{
    /// <summary>
    /// <c>budget.budget_role_query_field</c> (requirement 1): a role-to-query-field assignment (formerly the
    /// arrays inside each <c>BudgetRights.QueryFieldRoles</c> entry). Unique per (role, field); FKs RESTRICT.
    /// </summary>
    [Table("budget_role_query_field")]
    public class BudgetRoleQueryField : ActorStampedEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("role_id")]
        public long RoleId { get; set; }

        [Column("query_field_id")]
        public long QueryFieldId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public BudgetRole? Role { get; set; }

        [ForeignKey(nameof(QueryFieldId))]
        public BudgetQueryField? QueryField { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
