using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Entities
{
    /// <summary>
    /// T-26 <c>budget.admin_approval_version</c>: every version of an approval's particulars, the original
    /// being version 1 (DDD T-26, AAP-007, AAP-008). Append-only (DB-27); numbered from 1 without gap (DB-06);
    /// the reference is not used by another approval of the Division (DB-07); the copy is of evidence class
    /// (DB-08, VR-10); the account code is not marked not for use (DB-03). Written on Submit only.
    /// </summary>
    [Table("admin_approval_version")]
    public class AdminApprovalVersion : CreationStampedEntity
    {
        [Column("admin_approval_id")]
        public long AdminApprovalId { get; set; }
        [Column("version_no")]
        public short VersionNo { get; set; }
        /// <summary>Column 2.</summary>
        [Column("name_of_work")]
        [MaxLength(500)]
        public string NameOfWork { get; set; } = string.Empty;
        /// <summary>Column 3 (DB-07).</summary>
        [Column("aa_reference_no")]
        [MaxLength(50)]
        public string AaReferenceNo { get; set; } = string.Empty;
        /// <summary>Column 4; not later than the IST date of <c>created_at</c> (CHECK, BUD-014, BUD-056).</summary>
        [Column("date_of_approval")]
        public DateOnly DateOfApproval { get; set; }
        /// <summary>Column 5; FK T-07 account_code_id.</summary>
        [Column("account_code_id")]
        public long AccountCodeId { get; set; }
        /// <summary>Column 6; whole rupees, CHECK &gt; 0.</summary>
        [Column("amount_sanctioned")]
        public decimal AmountSanctioned { get; set; }
        /// <summary>Column 7; FK T-22, evidence class (DB-08). NOT NULL enforces VR-10 (M-04).</summary>
        [Column("aa_copy_attachment_id")]
        public long AaCopyAttachmentId { get; set; }
        /// <summary>Column 8.</summary>
        [Column("eoffice_ref")]
        [MaxLength(50)]
        public string? EofficeRef { get; set; }
        /// <summary>Column 9; SECRETARY, CE, SE or EE (BUD-054).</summary>
        [Column("competent_authority")]
        [MaxLength(10)]
        public string CompetentAuthority { get; set; } = string.Empty;
        /// <summary>Set by the module against T-19 (BUD-055).</summary>
        [Column("exceeds_delegation")]
        public bool ExceedsDelegation { get; set; }
        /// <summary>Not null where <see cref="ExceedsDelegation"/> (CHECK, M-27).</summary>
        [Column("delegation_justification")]
        [MaxLength(4000)]
        public string? DelegationJustification { get; set; }
    }
}
