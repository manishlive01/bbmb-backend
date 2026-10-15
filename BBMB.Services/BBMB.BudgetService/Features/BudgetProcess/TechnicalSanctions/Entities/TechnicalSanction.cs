using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Entities
{
    /// <summary>
    /// T-27 <c>budget.technical_sanction</c>: a technical sanction of a work as a continuing thing (DDD T-27).
    /// Its particulars are the versions in T-28 (DB-06). The tab equals the work's class and an AA-tab sanction
    /// is of the work's approval (DB-04); one sanction only against an AM&amp;R work (partial UK, M-06).
    /// <c>estimate_code</c> is written only by <c>budget.fn_record_estimate_code</c> and never altered (DB-05).
    /// </summary>
    [Table("technical_sanction")]
    public class TechnicalSanction : ActorStampedEntity
    {
        [Key]
        [Column("technical_sanction_id")]
        public long TechnicalSanctionId { get; set; }
        [Column("work_id")]
        public int WorkId { get; set; }
        /// <summary><see cref="SanctionTab"/>: AA or AMR, equal to the work's class (DB-04, BUD-068).</summary>
        [Column("tab")]
        [MaxLength(3)]
        public string Tab { get; set; } = string.Empty;
        /// <summary>Not null exactly when the tab is AA (CHECK, BUD-069, BUD-070).</summary>
        [Column("admin_approval_id")]
        public long? AdminApprovalId { get; set; }
        /// <summary>Column 1, allotted within the work's Division; unique there (trigger, DB-04).</summary>
        [Column("serial_no")]
        public int SerialNo { get; set; }
        [Column("current_version_no")]
        public short CurrentVersionNo { get; set; }
        /// <summary>Column 11; written only through <c>fn_record_estimate_code</c> (DB-05). Read-only to EF.</summary>
        [Column("estimate_code")]
        [MaxLength(30)]
        public string? EstimateCode { get; private set; }
        [Column("estimate_code_at")]
        public DateTimeOffset? EstimateCodeAt { get; private set; }
        [Timestamp]
        public uint Version { get; set; }
    }

    /// <summary>T-27 <c>tab</c> values (CHECK), equal to T-23 <c>work_class</c>.</summary>
    public static class SanctionTab
    {
        public const string Aa = "AA";
        public const string Amr = "AMR";
    }
}
