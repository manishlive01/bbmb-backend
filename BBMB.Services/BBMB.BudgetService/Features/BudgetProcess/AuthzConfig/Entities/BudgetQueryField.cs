using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities
{
    /// <summary>
    /// <c>budget.budget_query_field</c> (requirement 1): an available query-builder field, e.g. <c>category</c>
    /// (formerly the keys of <c>BudgetRights.QueryFieldRoles</c>). Role-to-field assignment lives in
    /// <see cref="BudgetRoleQueryField"/>.
    /// </summary>
    [Table("budget_query_field")]
    public class BudgetQueryField : ActorStampedEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("field_name")]
        [MaxLength(100)]
        public string FieldName { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Timestamp]
        public uint Version { get; set; }
    }
}
