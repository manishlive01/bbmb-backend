using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Entities
{
    /// <summary>
    /// T-28 <c>budget.technical_sanction_version</c>: every version of a sanction's particulars (DDD T-28,
    /// TSN-010). Append-only (DB-27); numbered from 1 without gap (DB-06); the TS number is not used by another
    /// sanction of the Division (DB-07); the copy is of evidence class (DB-08, VR-10); on the AA tab the current
    /// versions of the approval's sanctions total no more than TS_CEILING_PCT of its current amount, checked at
    /// commit (DB-09, VR-02, M-06).
    /// </summary>
    [Table("technical_sanction_version")]
    public class TechnicalSanctionVersion : CreationStampedEntity
    {
        [Column("technical_sanction_id")]
        public long TechnicalSanctionId { get; set; }
        [Column("version_no")]
        public short VersionNo { get; set; }
        /// <summary>Column 6.</summary>
        [Column("name_of_estimate")]
        [MaxLength(500)]
        public string NameOfEstimate { get; set; } = string.Empty;
        /// <summary>Column 7 (DB-07).</summary>
        [Column("ts_number")]
        [MaxLength(50)]
        public string TsNumber { get; set; } = string.Empty;
        /// <summary>Column 8; CHECK as T-26 date_of_approval.</summary>
        [Column("ts_date")]
        public DateOnly TsDate { get; set; }
        /// <summary>Column 9; whole rupees, CHECK &gt; 0.</summary>
        [Column("ts_amount")]
        public decimal TsAmount { get; set; }
        /// <summary>Column 10; FK T-22, evidence class (DB-08). NOT NULL enforces VR-10 (M-07).</summary>
        [Column("ts_copy_attachment_id")]
        public long TsCopyAttachmentId { get; set; }
    }
}
