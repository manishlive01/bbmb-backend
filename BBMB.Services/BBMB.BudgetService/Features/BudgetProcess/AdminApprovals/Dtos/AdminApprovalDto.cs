namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Dtos
{
    /// <summary>An approval with every version in the order recorded (BUD-063) and the copy link of each (BUD-060).</summary>
    public sealed class AdminApprovalDto
    {
        public long AdminApprovalId { get; set; }
        public int WorkId { get; set; }
        public long DivisionOfficeId { get; set; }
        public int SerialNo { get; set; }
        public short CurrentVersionNo { get; set; }
        public List<AdminApprovalVersionDto> Versions { get; set; } = [];
    }

    public sealed class AdminApprovalVersionDto
    {
        public short VersionNo { get; set; }
        public string NameOfWork { get; set; } = string.Empty;
        public string AaReferenceNo { get; set; } = string.Empty;
        public DateOnly DateOfApproval { get; set; }
        public long AccountCodeId { get; set; }
        public decimal AmountSanctioned { get; set; }
        public long AaCopyAttachmentId { get; set; }
        /// <summary>Download route of the copy (design §8.2), checked again on download.</summary>
        public string AaCopyUrl => $"/api/budget-attachments/{AaCopyAttachmentId}";
        public string? EofficeRef { get; set; }
        public string CompetentAuthority { get; set; } = string.Empty;
        public bool ExceedsDelegation { get; set; }
        public string? DelegationJustification { get; set; }
        public DateTimeOffset RecordedAt { get; set; }
    }
}
