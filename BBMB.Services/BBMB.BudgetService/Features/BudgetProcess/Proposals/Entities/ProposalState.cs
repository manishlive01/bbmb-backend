using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities
{
    /// <summary>
    /// T-15 <c>budget.state</c>: the sixteen proposal states S-01 to S-16 (SRS §5, DDD T-15, design §6.1).
    /// Seeded by migration under the migration actor; no write endpoint; append-only.
    /// </summary>
    [Table("state")]
    public class ProposalState
    {
        [Key]
        [Column("state_code")]
        [MaxLength(4)]
        public string StateCode { get; set; } = string.Empty;

        [Column("state_name")]
        [MaxLength(100)]
        public string StateName { get; set; } = string.Empty;

        /// <summary>Nominal T-13 level holding a proposal in this state; the actual holder is proposal.holder_office_id.</summary>
        [Column("holder_level")]
        [MaxLength(12)]
        public string HolderLevel { get; set; } = string.Empty;

        /// <summary>The holder may alter figures (S-01, S-03, S-04, S-06, S-08, S-10; S-10 WORKS only, DB-11).</summary>
        [Column("figures_open")]
        public bool FiguresOpen { get; set; }
    }
}
