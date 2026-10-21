using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities
{
    /// <summary>
    /// T-33 <c>budget.proposal_movement</c>: the movement log, append-only (DDD T-33). The triple
    /// (from_state, action_code, to_state) references T-16 once BudgetP1StateTransitionSeed lands.
    /// </summary>
    [Table("proposal_movement")]
    public class ProposalMovement : CreationStampedEntity
    {
        [Key]
        [Column("movement_id")]
        public long MovementId { get; set; }

        [Column("proposal_id")]
        public long ProposalId { get; set; }

        [Column("from_state")]
        [MaxLength(4)]
        public string FromState { get; set; } = string.Empty;

        [Column("action_code")]
        [MaxLength(20)]
        public string ActionCode { get; set; } = string.Empty;

        [Column("to_state")]
        [MaxLength(4)]
        public string ToState { get; set; } = string.Empty;

        [Column("from_office_id")]
        public long FromOfficeId { get; set; }

        [Column("to_office_id")]
        public long ToOfficeId { get; set; }

        /// <summary>Required where T-16 remark_required (DB-12, M-12).</summary>
        [Column("remark")]
        [MaxLength(4000)]
        public string? Remark { get; set; }

        [Column("role_code")]
        [MaxLength(50)]
        public string RoleCode { get; set; } = string.Empty;

        [Column("on_behalf_of_post")]
        [MaxLength(30)]
        public string? OnBehalfOfPost { get; set; }

        /// <summary>BUD-230. Never null: IPAddress.None when the connection has no remote address (§4.3).</summary>
        [Column("client_ip")]
        public IPAddress ClientIp { get; set; } = IPAddress.None;
    }
}
