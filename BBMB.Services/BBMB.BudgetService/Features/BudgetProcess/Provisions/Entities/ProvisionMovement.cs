using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities
{
    /// <summary>
    /// T-42 <c>budget.provision_movement</c> (DDD §5.7): the provision ledger, append-only. The amount available
    /// on a provision is the sum of its movements (V-02). An ALLOCATION movement is refused for a frozen cycle
    /// (DB-19); the movements of one re-appropriation or one unforeseen authorisation sum to nil, checked at
    /// commit (DB-21, BUD-204); no movement is accepted against a provision of a closed year (DB-22, BUD-251).
    /// </summary>
    [Table("provision_movement")]
    public class ProvisionMovement : CreationStampedEntity
    {
        [Column("provision_movement_id")]
        public long ProvisionMovementId { get; set; }

        /// <summary>FK T-41. Index (provision).</summary>
        [Column("provision_id")]
        public long ProvisionId { get; set; }

        /// <summary>CHECK in ALLOCATION, REAPPROP_IN, REAPPROP_OUT, UNFORESEEN_IN, UNFORESEEN_OUT.</summary>
        [Required]
        [MaxLength(14)]
        [Column("movement_type")]
        public string MovementType { get; set; } = string.Empty;

        /// <summary>Signed: positive for ALLOCATION and the two IN types, negative for the two OUT types (CHECK).</summary>
        [Column("amount")]
        public decimal Amount { get; set; }

        /// <summary>FK T-40. Not null exactly for ALLOCATION (CHECK).</summary>
        [Column("allocation_id")]
        public long? AllocationId { get; set; }

        /// <summary>FK T-46. Not null exactly for REAPPROP_IN and _OUT (CHECK). Added in P5.</summary>
        [Column("reappropriation_id")]
        public long? ReappropriationId { get; set; }

        /// <summary>FK T-50. Not null exactly for UNFORESEEN_IN and _OUT (CHECK).</summary>
        [Column("unforeseen_authorisation_id")]
        public long? UnforeseenAuthorisationId { get; set; }
    }

    /// <summary>T-42 <c>movement_type</c> values (CHECK).</summary>
    public static class ProvisionMovementType
    {
        public const string Allocation = "ALLOCATION";
        public const string ReappropIn = "REAPPROP_IN";
        public const string ReappropOut = "REAPPROP_OUT";
        public const string UnforeseenIn = "UNFORESEEN_IN";
        public const string UnforeseenOut = "UNFORESEEN_OUT";
    }
}
