namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create.Dtos
{
    /// <summary>Body of <c>POST /api/admin-approvals</c> (F-03, design §8.4). The serial is allotted, never entered (BUD-053).</summary>
    public sealed class CreateAdminApprovalDto
    {
        /// <summary>An existing AA work of the actor's Division; exactly one of <see cref="WorkId"/> and <see cref="NewWork"/>.</summary>
        public int? WorkId { get; set; }
        public NewAaWorkDto? NewWork { get; set; }
        public string NameOfWork { get; set; } = string.Empty;
        public string AaReferenceNo { get; set; } = string.Empty;
        public DateOnly DateOfApproval { get; set; }
        public long AccountCodeId { get; set; }
        public decimal AmountSanctioned { get; set; }
        /// <summary>The scanned copy (EVIDENCE, uploaded by the actor). Required (M-04, VR-10).</summary>
        public long? AaCopyAttachmentId { get; set; }
        public string? EofficeRef { get; set; }
        /// <summary>SECRETARY, CE, SE or EE (BUD-054).</summary>
        public string CompetentAuthority { get; set; } = string.Empty;
        /// <summary>Required to save an approval above the authority's delegated power (BUD-055, M-27).</summary>
        public string? DelegationJustification { get; set; }
    }

    /// <summary>A work created with the approval (<c>entered_via = AA_ENTRY</c>); the class is AA.</summary>
    public sealed class NewAaWorkDto
    {
        public string WorkClass { get; set; } = "AA";
    }
}
