using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Entities
{
    /// <summary>
    /// T-18 <c>budget.expenditure_source_setting</c>: whether the expenditure and receipt columns of an office
    /// are keyed or drawn for a financial year (DDD T-18, BUD-025, BUD-085, BUD-312). Append-only: a change is a
    /// new row with a later <c>effective_from</c>; the row in force is the latest at or before the instant.
    /// The absence of any row means KEYED, the Financial Accounting module not yet being available. Written only
    /// by the IT Cell role (application). The [C] actor columns are trigger-filled.
    /// </summary>
    [Table("expenditure_source_setting")]
    public class ExpenditureSourceSetting : CreationStampedEntity
    {
        [Key]
        [Column("source_setting_id")]
        public long SourceSettingId { get; set; }

        /// <summary>FK T-13.</summary>
        [Column("office_id")]
        public long OfficeId { get; set; }

        /// <summary>FK <c>master.fa_financial_years</c>.</summary>
        [Column("fin_year")]
        public int FinYear { get; set; }

        /// <summary>KEYED or DRAWN (CHECK).</summary>
        [Column("mode")]
        [MaxLength(5)]
        public string Mode { get; set; } = string.Empty;

        /// <summary>UK (office, year, effective_from).</summary>
        [Column("effective_from")]
        public DateTimeOffset EffectiveFrom { get; set; }
    }

    /// <summary>T-18 <c>mode</c> values (CHECK).</summary>
    public static class ExpenditureSourceMode
    {
        public const string Keyed = "KEYED";
        public const string Drawn = "DRAWN";
    }
}
