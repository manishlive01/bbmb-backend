using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Shared.Entities
{
    /// <summary>
    /// [C] actor columns of DDD §1.7 (append-only tables): filled by trigger <c>budget.fn_require_actor</c>
    /// from the transaction's actor context, never by the application (DB-28). Mapped read-only through
    /// <c>BudgetContext.ConfigureActorColumns</c>.
    /// </summary>
    public abstract class CreationStampedEntity
    {
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>employee.employee(id).</summary>
        [Column("created_by")]
        public Guid CreatedBy { get; private set; }

        /// <summary>employee.employee_charge(id).</summary>
        [Column("created_charge_id")]
        public Guid CreatedChargeId { get; private set; }
    }
}
