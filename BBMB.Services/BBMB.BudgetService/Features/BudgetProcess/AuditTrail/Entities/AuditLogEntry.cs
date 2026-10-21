using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Entities
{
    /// <summary>
    /// T-51 <c>budget.audit_log</c>: the audit trail (DDD T-51, §7.2; design §5.3). Append-only and written
    /// only by trigger <c>budget.fn_audit_row</c>; the application reads it (F-27) and never writes it.
    /// </summary>
    [Table("audit_log")]
    public class AuditLogEntry
    {
        [Key]
        [Column("audit_id")]
        public long AuditId { get; set; }

        [Column("occurred_at")]
        public DateTimeOffset OccurredAt { get; set; }

        [Column("transaction_id")]
        public long TransactionId { get; set; }

        [Column("table_name")]
        [MaxLength(63)]
        public string TableName { get; set; } = string.Empty;

        [Column("record_key")]
        [MaxLength(100)]
        public string RecordKey { get; set; } = string.Empty;

        /// <summary>I or U.</summary>
        [Column("operation")]
        [MaxLength(1)]
        public string Operation { get; set; } = string.Empty;

        [Column("column_name")]
        [MaxLength(63)]
        public string? ColumnName { get; set; }

        [Column("old_value", TypeName = "jsonb")]
        public string? OldValue { get; set; }

        [Column("new_value", TypeName = "jsonb")]
        public string? NewValue { get; set; }

        /// <summary>
        /// Business action from the actor context. Widened from DDD varchar(30) to varchar(100): the design's
        /// own action codes (<c>MIGRATION:&lt;migration name&gt;</c>, <c>BULK_UPLOAD:&lt;type&gt;</c>) exceed 30.
        /// </summary>
        [Column("action_code")]
        [MaxLength(100)]
        public string? ActionCode { get; set; }

        [Column("actor_employee_id")]
        public Guid ActorEmployeeId { get; set; }

        [Column("actor_name")]
        [MaxLength(200)]
        public string ActorName { get; set; } = string.Empty;

        [Column("actor_charge_id")]
        public Guid ActorChargeId { get; set; }

        [Column("actor_office_id")]
        public long ActorOfficeId { get; set; }

        [Column("actor_role_code")]
        [MaxLength(50)]
        public string ActorRoleCode { get; set; } = string.Empty;

        [Column("on_behalf_of_post")]
        [MaxLength(30)]
        public string? OnBehalfOfPost { get; set; }

        [Column("client_ip")]
        public IPAddress? ClientIp { get; set; }

        [Column("request_id")]
        public Guid? RequestId { get; set; }
    }
}
