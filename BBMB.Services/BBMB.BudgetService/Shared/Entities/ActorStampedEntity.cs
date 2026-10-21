using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Shared.Entities
{
    /// <summary>
    /// [A] actor columns of DDD §1.7 (mutable tables): the [C] creation columns plus the last update.
    /// Trigger-filled (DB-28); the application never writes them.
    /// </summary>
    public abstract class ActorStampedEntity : CreationStampedEntity
    {
        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; private set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; private set; }

        [Column("updated_charge_id")]
        public Guid? UpdatedChargeId { get; private set; }
    }
}
