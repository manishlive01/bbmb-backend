using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Entities
{
    /// <summary>
    /// T-25 <c>budget.admin_approval</c>: the administrative approval of a work as a continuing thing
    /// (DDD T-25). Its particulars are the versions in T-26; a revision is a further version, never a second
    /// approval (one approval per work, UK). <c>current_version_no</c> equals the highest T-26 version at
    /// commit (DB-06). No PUT and no DELETE exist (design §4.7).
    /// </summary>
    [Table("admin_approval")]
    public class AdminApproval : ActorStampedEntity
    {
        [Key]
        [Column("admin_approval_id")]
        public long AdminApprovalId { get; set; }
        /// <summary>FK work; UK (one approval per work).</summary>
        [Column("work_id")]
        public int WorkId { get; set; }
        /// <summary>The DIVISION office of the work's Division (DB-01, DB-04, AAP-003).</summary>
        [Column("division_office_id")]
        public long DivisionOfficeId { get; set; }
        /// <summary>F-03 column 1, allotted by the module within the Division; UK (Division, serial).</summary>
        [Column("serial_no")]
        public int SerialNo { get; set; }
        [Column("current_version_no")]
        public short CurrentVersionNo { get; set; }
        [Timestamp]
        public uint Version { get; set; }
    }
}
