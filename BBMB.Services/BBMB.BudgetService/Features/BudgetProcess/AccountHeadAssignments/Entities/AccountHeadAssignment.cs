using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Entities
{
    /// <summary>
    /// T-21 <c>budget.account_head_assignment</c>: the heads an office may demand against (DDD T-21, F-17, VR-06,
    /// BUD-168 to BUD-171). The office is a DIVISION, CIRCLE or CE (DB-01). Removal is by marking
    /// <c>removed_at</c>, never by deletion, and is refused while a demand line or provision exists for the office
    /// and head (DB-02, M-15). One live row per office and head (partial UK).
    /// </summary>
    [Table("account_head_assignment")]
    public class AccountHeadAssignment : ActorStampedEntity
    {
        [Key]
        [Column("assignment_id")]
        public long AssignmentId { get; set; }
        [Column("office_id")]
        public long OfficeId { get; set; }
        /// <summary>FK T-07 account_code_id (SQL; the unique key, not the six-column PK).</summary>
        [Column("account_code_id")]
        public long AccountCodeId { get; set; }
        /// <summary><see cref="AssignmentSource"/> (CHECK).</summary>
        [Column("source")]
        [MaxLength(8)]
        public string Source { get; set; } = string.Empty;
        [Column("removed_at")]
        public DateTimeOffset? RemovedAt { get; set; }
        /// <summary>employee.employee(id); set with <see cref="RemovedAt"/> from the actor context by trigger.</summary>
        [Column("removed_by")]
        public Guid? RemovedBy { get; set; }
        [Timestamp]
        public uint Version { get; set; }
    }

    /// <summary>T-21 <c>source</c> values (CHECK, BUD-171).</summary>
    public static class AssignmentSource
    {
        public const string R04Load = "R04_LOAD";
        public const string Afa = "AFA";
    }
}
