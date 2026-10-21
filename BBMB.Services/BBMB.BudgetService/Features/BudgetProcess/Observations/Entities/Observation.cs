using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Entities
{
    /// <summary>
    /// T-32 <c>budget.observation</c>: a remark against a line, or at the foot of a proposal (DDD T-32).
    /// Fixed once written (DB-15), save the reply written once and, inside a RETURN, the two return columns
    /// set once each (design §5.5 row d). <see cref="RepliedAt"/> and <see cref="RepliedBy"/> are filled by
    /// the protect trigger from the actor context.
    /// </summary>
    [Table("observation")]
    public class Observation : CreationStampedEntity
    {
        [Key]
        [Column("observation_id")]
        public long ObservationId { get; set; }

        [Column("proposal_id")]
        public long ProposalId { get; set; }

        /// <summary>Null for a remark at the foot of the proposal (BUD-128).</summary>
        [Column("demand_line_id")]
        public long? DemandLineId { get; set; }

        [Column("level_office_id")]
        public long LevelOfficeId { get; set; }

        [Column("observation_text")]
        [MaxLength(4000)]
        public string ObservationText { get; set; } = string.Empty;

        [Column("selected_for_return")]
        public bool SelectedForReturn { get; set; }

        [Column("returned_by_movement_id")]
        public long? ReturnedByMovementId { get; set; }

        [Column("reply_text")]
        [MaxLength(4000)]
        public string? ReplyText { get; set; }

        [Column("replied_at")]
        public DateTimeOffset? RepliedAt { get; private set; }

        [Column("replied_by")]
        public Guid? RepliedBy { get; private set; }
    }
}
