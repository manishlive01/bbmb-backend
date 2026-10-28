using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.AuthzConfig.Entities
{
    /// <summary>Lifecycle status of a <see cref="BudgetAdminRoleAssignment"/> (CHECK <c>ck_admin_status</c>).</summary>
    public static class AdminAssignmentStatus
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Revoked = "REVOKED";

        public static readonly IReadOnlyList<string> All = [Pending, Approved, Revoked];
    }

    /// <summary>
    /// <c>budget.budget_admin_role_assignment</c> (requirement 4): the document-backed, approved record that
    /// makes a <see cref="BudgetRole"/> an <b>effective</b> Budget administrator. A role is an effective admin
    /// iff <c>budget_role.is_admin</c> is true AND it has a row here with <c>status = APPROVED</c> and a
    /// <see cref="DocumentRef"/>. Until the signed document is present and the assignment is approved, the role
    /// receives only the rights explicitly granted in <see cref="BudgetRoleRight"/>. The DB owns the
    /// "APPROVED ⇒ has document + approver" invariant via <c>ck_admin_approved_has_doc</c>.
    /// </summary>
    [Table("budget_admin_role_assignment")]
    public class BudgetAdminRoleAssignment : ActorStampedEntity
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("role_id")]
        public long RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public BudgetRole? Role { get; set; }

        /// <summary>One of <see cref="AdminAssignmentStatus"/>.</summary>
        [Column("status")]
        [MaxLength(16)]
        public string Status { get; set; } = AdminAssignmentStatus.Pending;

        /// <summary>Reference to the uploaded signed document (e-Office ref / attachment storage key).</summary>
        [Column("document_ref")]
        [MaxLength(200)]
        public string? DocumentRef { get; set; }

        [Column("uploaded_by")]
        public Guid? UploadedBy { get; set; }

        [Column("uploaded_at")]
        public DateTimeOffset? UploadedAt { get; set; }

        [Column("approved_by")]
        public Guid? ApprovedBy { get; set; }

        [Column("approved_at")]
        public DateTimeOffset? ApprovedAt { get; set; }

        [Column("revoked_by")]
        public Guid? RevokedBy { get; set; }

        [Column("revoked_at")]
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>Reason captured at revocation (free text).</summary>
        [Column("revoke_reason")]
        [MaxLength(500)]
        public string? RevokeReason { get; set; }

        [Timestamp]
        public uint Version { get; set; }

        /// <summary>The effective-admin predicate (design §1 "Effective admin status rule").</summary>
        public bool IsEffectiveAdmin() =>
            string.Equals(Status, AdminAssignmentStatus.Approved, StringComparison.Ordinal) &&
            !string.IsNullOrWhiteSpace(DocumentRef);
    }
}
