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

        [Required]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("closed")]
        public DateTime? Closed { get; set; }
    }
}
