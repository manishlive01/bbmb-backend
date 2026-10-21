using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.FinancialYears.Entities
{
    [Table("fa_financial_years")]
    public class FinancialYear
    {
        [Key]
        [Column("fin_year_closing")]
        public int FinYearClosing { get; set; }

        // DDD T-08: business dates are date (design §5.2, migration BudgetP1ReferenceStateAndAudit).
        // CHECK start_date = 1 April of the preceding year; end_date = 31 March of fin_year_closing.
        [Required]
        [Column("start_date")]
        public DateOnly StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateOnly EndDate { get; set; }

        [Column("closed")]
        public DateTime? Closed { get; set; }
    }
}
