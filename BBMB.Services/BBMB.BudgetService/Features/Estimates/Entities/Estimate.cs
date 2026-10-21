using BBMB.BudgetService.Features.Works.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Estimates.Entities
{
    [Table("estimate")]
    public class Estimate
    {
        /// <summary>
        /// New key part after the P4 reset (design §4.6 step 2): the estimate is keyed on <c>(work_id, estimate_code)</c>
        /// with an FK to <c>work.id</c>. Backfilled from the old <c>work_code</c> match during the migration.
        /// </summary>
        [Column("work_id")]
        public int WorkId { get; set; }

        // Kept as a plain int column through all of P4 (design §4.6 step 2): no FK, no key, no longer written.
        // It is the only independent record of which work each estimate belonged to before the reset, so it stays
        // until the step-6 exit guard has passed in every environment. Removal is a later migration (OI-46).
        [Obsolete("P4 §4.6: the estimate is now keyed on WorkId; work_code is retained read-only until OI-46.")]
        [MaxLength(8)]
        [Column("work_code")]
        public int WorkCode { get; set; }

        [Required]
        [MaxLength(10)]
        [Column("estimate_code")]
        public string EstimateCode { get; set; } = string.Empty;

        [MaxLength(600)]
        [Column("estimate_desc")]
        public string? EstimateDesc { get; set; }

        // Column type budget.rupees, configured in BudgetContext (design §4.5).
        [Column("estimate_amt")]
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
