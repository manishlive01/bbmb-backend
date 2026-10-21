namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise.Dtos
{
    /// <summary>
    /// Body of <c>POST /api/admin-approvals/{id}/revisions</c> (F-04, design §8.4): the particulars of the new
    /// version, the copy of the revised approval included. The work is the approval's and does not change.
    /// </summary>
    public sealed class ReviseAdminApprovalDto
    {
        public string NameOfWork { get; set; } = string.Empty;
        public string AaReferenceNo { get; set; } = string.Empty;
        public DateOnly DateOfApproval { get; set; }
        public long AccountCodeId { get; set; }
        public decimal AmountSanctioned { get; set; }
        public long? AaCopyAttachmentId { get; set; }
        public string? EofficeRef { get; set; }
        public string CompetentAuthority { get; set; } = string.Empty;
        public string? DelegationJustification { get; set; }
    }
}
