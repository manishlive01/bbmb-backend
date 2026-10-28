using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Entities
{
    /// <summary>
    /// T-48 <c>budget.unforeseen_request</c> (DDD §5.8, F-30): the proposal to draw on unforeseen provision. It
    /// does not follow the T-15 states; it carries its own status and movement log (T-49). The work has a
    /// recorded approval and no work code (DB-25, BUD-294). At least one amount with its provision (CHECK).
    /// </summary>
    [Table("unforeseen_request")]
    public class UnforeseenRequest : ActorStampedEntity
    {
        [Column("unforeseen_request_id")]
        public long UnforeseenRequestId { get; set; }

        /// <summary>FK T-23; UK. A work carrying a recorded approval and no work code (DB-25, BUD-294).</summary>
        [Column("work_id")]
        public int WorkId { get; set; }

        /// <summary>FK T-13.</summary>
        [Column("division_office_id")]
        public long DivisionOfficeId { get; set; }

        /// <summary>FK T-41. The UNFORESEEN provision drawn upon for the current year (BUD-297).</summary>
        [Column("current_provision_id")]
        public long? CurrentProvisionId { get; set; }

        /// <summary>Required under the revised estimate.</summary>
        [Column("amount_current")]
        public decimal? AmountCurrent { get; set; }

        /// <summary>FK T-41. The UNFORESEEN provision for the coming year, where drawn upon.</summary>
        [Column("next_provision_id")]
        public long? NextProvisionId { get; set; }

        /// <summary>Required under the estimate of the coming year. At least one amount with its provision (CHECK).</summary>
        [Column("amount_next")]
        public decimal? AmountNext { get; set; }

        /// <summary>CHECK in OPEN, AT_CIRCLE, AT_CE, RETURNED, AUTHORISED.</summary>
        [Required]
        [MaxLength(10)]
        [Column("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>FK T-13.</summary>
        [Column("holder_office_id")]
        public long HolderOfficeId { get; set; }

        [Timestamp]
        public uint Version { get; set; }
    }

    /// <summary>T-48 <c>status</c> values (CHECK) and the T-49 permitted-move pairs (design §6.3).</summary>
    public static class UnforeseenStatus
    {
        public const string Open = "OPEN";
        public const string AtCircle = "AT_CIRCLE";
        public const string AtCe = "AT_CE";
        public const string Returned = "RETURNED";
        public const string Authorised = "AUTHORISED";
    }
}
