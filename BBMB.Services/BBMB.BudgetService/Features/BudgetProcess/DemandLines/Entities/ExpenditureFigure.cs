using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities
{
    /// <summary>
    /// T-35 <c>budget.expenditure_figure</c>: expenditure and receipts keyed in place of Financial Accounting,
    /// append-only (DDD T-35). The latest row per (subject, nature, year, period) is current; a keyed figure is
    /// never replaced by a drawn one (BUD-025).
    /// </summary>
    [Table("expenditure_figure")]
    public class ExpenditureFigure : CreationStampedEntity
    {
        [Key]
        [Column("expenditure_figure_id")]
        public long ExpenditureFigureId { get; set; }

        /// <summary>WORK or HEAD.</summary>
        [Column("subject_type")]
        [MaxLength(4)]
        public string SubjectType { get; set; } = string.Empty;

        [Column("work_id")]
        public int? WorkId { get; set; }

        [Column("office_id")]
        public long? OfficeId { get; set; }

        [Column("account_code_id")]
        public long? AccountCodeId { get; set; }

        /// <summary>EXPENDITURE or RECEIPT.</summary>
        [Column("nature")]
        [MaxLength(11)]
        public string Nature { get; set; } = string.Empty;

        [Column("fin_year")]
        public int FinYear { get; set; }

        /// <summary>FULL_YEAR, UP_TO_MONTH or UP_TO_DATE.</summary>
        [Column("period")]
        [MaxLength(11)]
        public string Period { get; set; } = string.Empty;

        /// <summary>First day of the month; exactly for UP_TO_MONTH (BUD-141).</summary>
        [Column("up_to_month")]
        public DateOnly? UpToMonth { get; set; }

        /// <summary>budget.rupees_paise; whole rupees where KEYED (BUD-013), paise where DRAWN (BUD-266).</summary>
        [Column("amount")]
        public decimal Amount { get; set; }

        /// <summary>KEYED or DRAWN.</summary>
        [Column("source")]
        [MaxLength(5)]
        public string Source { get; set; } = string.Empty;

        [Column("as_at")]
        public DateTimeOffset AsAt { get; set; }
    }
}
