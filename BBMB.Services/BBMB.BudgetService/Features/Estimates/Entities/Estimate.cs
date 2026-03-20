using BBMB.BudgetService.Features.Works.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Estimates.Entities
{
    [Table("estimate")]
    public class Estimate
    {
        [Key]
        [Required]
        [MaxLength(8)]
        [Column("work_code")]
        public int WorkCode { get; set; }  

        [Key]
        [Required]
        [MaxLength(10)]
        [Column("estimate_code")]
        public string EstimateCode { get; set; } = string.Empty;

        [MaxLength(600)]
        [Column("estimate_desc")]
        public string? EstimateDesc { get; set; }

        [Column("estimate_amt")]
        [Precision(12, 2)]
        public decimal? EstimateAmt { get; set; }

        [MaxLength(600)]
        [Column("estimate_narr")]
        public string? EstimateNarr { get; set; }

        [Required]
        [Column("fin_year_end")]
        public int FinYearEnd { get; set; }

        [Required]
        [Column("division_code")]
        public int DivisionCode { get; set; }

        // Navigation properties for foreign keys
        public virtual Work? Work { get; set; }
        // Note: Division navigation property removed since division table uses composite key
        // but estimate only has division_code. Division validation is done manually in service.
    }
}
