using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Entities
{
    /// <summary>
    /// T-49 <c>budget.unforeseen_request_movement</c> (DDD §5.8): the request's own movement log, append-only.
    /// The (from_status, to_status) pair is checked against the permitted moves of design §6.3; a remark is
    /// required on a return (CHECK).
    /// </summary>
    [Table("unforeseen_request_movement")]
    public class UnforeseenRequestMovement : CreationStampedEntity
    {
        [Column("movement_id")]
        public long MovementId { get; set; }

        /// <summary>FK T-48.</summary>
        [Column("unforeseen_request_id")]
        public long UnforeseenRequestId { get; set; }

        /// <summary>CHECK on the pair against the permitted moves (design §6.3).</summary>
        [Required]
        [MaxLength(10)]
        [Column("from_status")]
        public string FromStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        [Column("to_status")]
        public string ToStatus { get; set; } = string.Empty;

        /// <summary>FK T-13.</summary>
        [Column("from_office_id")]
        public long FromOfficeId { get; set; }

        /// <summary>FK T-13.</summary>
        [Column("to_office_id")]
        public long ToOfficeId { get; set; }

        /// <summary>Required on a return (CHECK).</summary>
        [MaxLength(4000)]
        [Column("remark")]
        public string? Remark { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("role_code")]
        public string RoleCode { get; set; } = string.Empty;

        [Column("client_ip")]
        public IPAddress ClientIp { get; set; } = IPAddress.None;
    }
}
