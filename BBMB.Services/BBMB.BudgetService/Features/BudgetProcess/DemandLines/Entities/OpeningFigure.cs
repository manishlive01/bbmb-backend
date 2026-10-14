using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities
{
    /// <summary>
    /// T-36 <c>budget.opening_figure</c>: carried-forward figures keyed in the first cycle (DDD T-36). Accepted
    /// only in the first cycle and fixed once the origin proposal is first submitted (DB-16, BUD-306).
    /// </summary>
    [Table("opening_figure")]
    public class OpeningFigure : ActorStampedEntity
    {
        [Key]
        [Column("opening_figure_id")]
        public long OpeningFigureId { get; set; }

        [Column("cycle_id")]
        public long CycleId { get; set; }

        [Column("subject_type")]
        [MaxLength(4)]
        public string SubjectType { get; set; } = string.Empty;

        [Column("work_id")]
        public int? WorkId { get; set; }

        [Column("office_id")]
        public long? OfficeId { get; set; }

        [Column("account_code_id")]
        public long? AccountCodeId { get; set; }

        /// <summary>OBE_CURRENT (Table 3 col 9, Table 4 col 5) or RBE_PREVIOUS (Table 4 col 3).</summary>
        [Column("figure_kind")]
        [MaxLength(12)]
        public string FigureKind { get; set; } = string.Empty;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }
}
