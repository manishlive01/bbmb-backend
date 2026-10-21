using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Works.Entities
{
    /// <summary>
    /// DDD T-24 <c>budget.work_code_serial</c> (design §4.6 step 5): the per-(Division, year) serial of the work
    /// code. <c>budget.fn_generate_work_code</c> inserts the row for the Division and year if absent and increments
    /// <c>last_serial</c> in one <c>INSERT … ON CONFLICT DO UPDATE … RETURNING</c>, which holds the row lock until
    /// commit and so serialises concurrent allocations for the same Division and year (DV-03, BUD-314).
    /// </summary>
    [Table("work_code_serial")]
    public class WorkCodeSerial
    {
        /// <summary>CHECK between 1 and 999 (as T-12). Taken from master.division.division_code at generation (BUD-315).</summary>
        [Column("division_code")]
        public int DivisionCode { get; set; }

        /// <summary>FK master.fa_financial_years (held in schema <c>budget</c>, §5.5 row f).</summary>
        [Column("fin_year")]
        public int FinYear { get; set; }

        /// <summary>CHECK between 1 and 999. A serial beyond 999 fails the CHECK and no code is generated (BUD-314).</summary>
        [Column("last_serial")]
        public short LastSerial { get; set; }
    }
}
