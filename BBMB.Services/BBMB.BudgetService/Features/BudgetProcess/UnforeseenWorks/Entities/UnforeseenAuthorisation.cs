using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Entities
{
    /// <summary>
    /// T-50 <c>budget.unforeseen_authorisation</c> (DDD §5.8, F-30): the Chief Engineer's authorisation,
    /// append-only. The one authorisation the module creates rather than records. The row and the snapshot of
    /// the request as it stood are fixed once written (BUD-296). The amounts are not above the balance of the
    /// provision drawn upon (DB-26, VR-13, M-29). On insertion, in one transaction, the work code is generated,
    /// a WORK provision is opened for each year authorised, and balanced UNFORESEEN_OUT / UNFORESEEN_IN
    /// movements are written (DB-21).
    /// </summary>
    [Table("unforeseen_authorisation")]
    public class UnforeseenAuthorisation : CreationStampedEntity
    {
        [Column("unforeseen_authorisation_id")]
        public long UnforeseenAuthorisationId { get; set; }

        /// <summary>FK T-48; UK. A request standing AT_CE.</summary>
        [Column("unforeseen_request_id")]
        public long UnforeseenRequestId { get; set; }

        /// <summary>FK T-13. The CE of the charge of the Division (DB-01, BUD-295).</summary>
        [Column("ce_office_id")]
        public long CeOfficeId { get; set; }

        /// <summary>Not above the balance of the provision drawn upon (DB-26, VR-13, M-29).</summary>
        [Column("amount_current")]
        public decimal? AmountCurrent { get; set; }

        [Column("amount_next")]
        public decimal? AmountNext { get; set; }

        [Required]
        [MaxLength(4000)]
        [Column("reasons")]
        public string Reasons { get; set; } = string.Empty;

        /// <summary>The request, the approval version and the amounts as they stood (BUD-296).</summary>
        [Required]
        [Column("request_snapshot", TypeName = "jsonb")]
        public string RequestSnapshot { get; set; } = "{}";

        /// <summary>Equal to the code written to T-23 in the same transaction (BUD-298).</summary>
        [Required]
        [MaxLength(12)]
        [Column("work_code_generated")]
        public string WorkCodeGenerated { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("role_code")]
        public string RoleCode { get; set; } = string.Empty;

        [Column("client_ip")]
        public IPAddress ClientIp { get; set; } = IPAddress.None;
    }
}
