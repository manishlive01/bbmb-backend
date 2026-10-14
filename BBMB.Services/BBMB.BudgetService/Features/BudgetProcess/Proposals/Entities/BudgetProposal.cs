using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities
{
    /// <summary>
    /// T-29 <c>budget.proposal</c>: one category of one origin office for one cycle (DDD T-29, design §5.3).
    /// The state and holder change only together with a T-33 movement row (DB-10).
    /// </summary>
    [Table("proposal")]
    public class BudgetProposal : ActorStampedEntity
    {
        [Key]
        [Column("proposal_id")]
        public long ProposalId { get; set; }

        [Column("cycle_id")]
        public long CycleId { get; set; }

        [Column("category")]
        [MaxLength(8)]
        public string Category { get; set; } = string.Empty;

        [Column("origin_office_id")]
        public long OriginOfficeId { get; set; }

        [Column("state_code")]
        [MaxLength(4)]
        public string StateCode { get; set; } = ProposalStateCode.S01;

        [Column("holder_office_id")]
        public long HolderOfficeId { get; set; }

        /// <summary>Set on the first SUBMIT; closes opening entries (BUD-306, DB-16).</summary>
        [Column("first_submitted_at")]
        public DateTimeOffset? FirstSubmittedAt { get; set; }

        /// <summary>The officer holding the edit lease (BUD-015, design §4.10).</summary>
        [Column("edit_lease_by")]
        public Guid? EditLeaseBy { get; set; }

        [Column("edit_lease_expires_at")]
        public DateTimeOffset? EditLeaseExpiresAt { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
