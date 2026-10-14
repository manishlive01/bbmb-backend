using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Entities
{
    /// <summary>
    /// T-17 <c>budget.setting</c>: configurable limits, append-only (DDD T-17, design §5.3). Each change is a
    /// new row; the value in force is the latest row with <c>effective_from &lt;= now()</c> (BUD-242).
    /// </summary>
    [Table("setting")]
    public class BudgetSetting : CreationStampedEntity
    {
        [Key]
        [Column("setting_id")]
        public long SettingId { get; set; }

        [Column("setting_key")]
        [MaxLength(40)]
        public string SettingKey { get; set; } = string.Empty;

        /// <summary>For every key except UNFORESEEN_ACCOUNT_CODE (CHECK).</summary>
        [Column("numeric_value")]
        [Precision(15, 2)]
        public decimal? NumericValue { get; set; }

        /// <summary>
        /// T-07 account_code_id, for UNFORESEEN_ACCOUNT_CODE only. The FK to T-07 is added by
        /// BudgetP1ProposalSpineMasters, which creates <c>budget_head_master.account_code_id</c>.
        /// </summary>
        [Column("account_code_id")]
        public long? AccountCodeId { get; set; }

        [Column("effective_from")]
        public DateTimeOffset EffectiveFrom { get; set; }

        [Column("authority_ref")]
        [MaxLength(100)]
        public string? AuthorityRef { get; set; }
    }

    /// <summary>T-17 <c>setting_key</c> values (CHECK).</summary>
    public static class SettingKey
    {
        public const string TsCeilingPct = "TS_CEILING_PCT";
        public const string NoAaLimitRupees = "NO_AA_LIMIT_RUPEES";
        public const string AaDemandCeilingPct = "AA_DEMAND_CEILING_PCT";
        public const string AmrIncreasePct = "AMR_INCREASE_PCT";
        public const string UploadMaxLines = "UPLOAD_MAX_LINES";
        public const string UnforeseenAccountCode = "UNFORESEEN_ACCOUNT_CODE";
    }
}
