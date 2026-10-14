using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities
{
    /// <summary>
    /// T-14 <c>budget.budget_cycle</c>: one annual budget exercise (DDD T-14, design §5.3). The current cycle
    /// is the OPEN row with the greatest <see cref="CurrentFinYear"/> (design §5.3).
    /// </summary>
    [Table("budget_cycle")]
    public class BudgetCycle : ActorStampedEntity
    {
        [Key]
        [Column("cycle_id")]
        public long CycleId { get; set; }

        /// <summary>fa_financial_years.fin_year_closing of the year whose revised estimate is framed.</summary>
        [Column("current_fin_year")]
        public int CurrentFinYear { get; set; }

        /// <summary>The year whose original estimate is framed; CHECK = current + 1.</summary>
        [Column("next_fin_year")]
        public int NextFinYear { get; set; }

        [Column("is_first_cycle")]
        public bool IsFirstCycle { get; set; }

        [Column("status")]
        [MaxLength(8)]
        public string Status { get; set; } = CycleStatus.Open;

        [Timestamp]
        public uint Version { get; set; }
    }

    /// <summary>T-14 <c>status</c> values (CHECK). FROZEN is set by T-44 only (DB-19).</summary>
    public static class CycleStatus
    {
        public const string Open = "OPEN";
        public const string Frozen = "FROZEN";
        public const string Closed = "CLOSED";
    }
}
