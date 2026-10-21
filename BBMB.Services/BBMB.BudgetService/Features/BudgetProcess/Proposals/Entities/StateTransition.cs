using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities
{
    /// <summary>
    /// T-16 <c>budget.state_transition</c>: every permitted move as (from state, action, to state) (DDD T-16,
    /// design §6.2). PK on the three columns (design §5.5 row l, CR-DDD-10); T-33 references it by the same
    /// three columns. Seeded by BudgetP1StateTransitionSeed under gate p1_state_model; no write endpoint.
    /// </summary>
    [Table("state_transition")]
    public class StateTransition
    {
        [Column("from_state")]
        [MaxLength(4)]
        public string FromState { get; set; } = string.Empty;

        [Column("action_code")]
        [MaxLength(20)]
        public string ActionCode { get; set; } = string.Empty;

        [Column("to_state")]
        [MaxLength(4)]
        public string ToState { get; set; } = string.Empty;

        /// <summary>True for RETURN, SEND_BACK, REFER_BACK, REOPEN (DB-12, BUD-120).</summary>
        [Column("remark_required")]
        public bool RemarkRequired { get; set; }
    }
}
