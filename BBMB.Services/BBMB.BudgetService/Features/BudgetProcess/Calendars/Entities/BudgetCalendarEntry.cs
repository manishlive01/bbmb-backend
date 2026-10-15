using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Entities
{
    /// <summary>
    /// T-20 <c>budget.calendar</c>: the last date for action at each office level of a cycle (DDD T-20, F-23,
    /// WKB-017, BUD-210 to BUD-213). The PK is (cycle_id, office_level). A lapsed date warns (M-26) and refuses
    /// nothing (BUD-212). The row is mutable (the date is revised); the [A] actor columns are trigger-filled.
    /// </summary>
    [Table("calendar")]
    public class BudgetCalendarEntry : ActorStampedEntity
    {
        [Column("cycle_id")]
        public long CycleId { get; set; }

        /// <summary>An office level as T-13 <c>office_level</c>, save IT_CELL (CHECK). PK part.</summary>
        [Column("office_level")]
        [MaxLength(12)]
        public string OfficeLevel { get; set; } = string.Empty;

        [Column("last_date")]
        public DateOnly LastDate { get; set; }
    }
}
