using System.ComponentModel.DataAnnotations.Schema;
namespace BBMB.BudgetService.Features.BudgetProcess.Reports.Entities
{
    /// <summary>
    /// V-01 <c>budget.v_manual_of_accounts_codes</c> (DDD §8.1, F-02, BUD-048 to BUD-050): every head of account as at
    /// the moment of the query, with the six codes and nomenclatures, the complete code, the ACC-009 flag, origin,
    /// creation particulars and the not-for-use mark. Read-only (keyless).
    /// </summary>
    public class ManualOfAccountsCodeRow
    {
        [Column("account_code_id")] public long AccountCodeId { get; set; }
        [Column("major_acc_code")] public string MajorAccCode { get; set; } = string.Empty;
        [Column("major_acc_desc")] public string? MajorAccDesc { get; set; }
        [Column("sub_major_acc_code")] public string SubMajorAccCode { get; set; } = string.Empty;
        [Column("sub_major_acc_desc")] public string? SubMajorAccDesc { get; set; }
        [Column("minor_acc_code")] public string MinorAccCode { get; set; } = string.Empty;
        [Column("minor_acc_desc")] public string? MinorAccDesc { get; set; }
        [Column("sub_minor_acc_code")] public string SubMinorAccCode { get; set; } = string.Empty;
        [Column("sub_minor_acc_desc")] public string? SubMinorAccDesc { get; set; }
        [Column("detail_acc_code")] public string DetailAccCode { get; set; } = string.Empty;
        [Column("detail_acc_desc")] public string? DetailAccDesc { get; set; }
        [Column("object_acc_code")] public string ObjectAccCode { get; set; } = string.Empty;
        [Column("object_acc_desc")] public string? ObjectAccDesc { get; set; }
        [Column("complete_code")] public string CompleteCode { get; set; } = string.Empty;
        [Column("head_desc")] public string HeadDesc { get; set; } = string.Empty;
        [Column("party_emp_required")] public string PartyEmpRequired { get; set; } = string.Empty;
        [Column("origin")] public string Origin { get; set; } = string.Empty;
        [Column("eoffice_approval_ref")] public string? EofficeApprovalRef { get; set; }
        [Column("created_at")] public DateTimeOffset? CreatedAt { get; set; }
        [Column("created_by")] public Guid? CreatedBy { get; set; }
        [Column("created_by_name")] public string? CreatedByName { get; set; }
        [Column("not_for_use")] public bool NotForUse { get; set; }
        [Column("not_for_use_ref")] public string? NotForUseRef { get; set; }
        [Column("not_for_use_at")] public DateTimeOffset? NotForUseAt { get; set; }
    }
}
