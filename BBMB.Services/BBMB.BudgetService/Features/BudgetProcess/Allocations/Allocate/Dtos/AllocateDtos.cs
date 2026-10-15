namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Allocate.Dtos
{
    /// <summary>Design §8.6 POST allocations request body.</summary>
    public sealed class AllocateRequestDto
    {
        public string Body { get; set; } = string.Empty;
        public string MinutesReference { get; set; } = string.Empty;
        public DateOnly MinutesDate { get; set; }
        public long MinutesAttachmentId { get; set; }
        public string FaCaoApprovalRef { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        /// <summary>The counts the Chief Engineer confirmed from the preview (BUD-189); re-checked here.</summary>
        public int ConfirmLinesToActivate { get; set; }
        public decimal ConfirmTotalToActivate { get; set; }
    }

    /// <summary>Design §8.6 allocation success.</summary>
    public sealed class AllocationResponseDto
    {
        public long AllocationId { get; set; }
        public int LinesActivated { get; set; }
        public decimal TotalActivated { get; set; }
        public List<string> WorkCodesGenerated { get; set; } = [];
    }

    /// <summary>
    /// Returned with M-19 when the recomputed plan differs from the confirmed counts (SRS §7 has no message for
    /// "figures changed since preview", catalogue gap OI-13): the client is told the real cause.
    /// </summary>
    public sealed class AllocationPreviewMismatchDto
    {
        public PreviewCounts Expected { get; set; } = new();
        public PreviewCounts Actual { get; set; } = new();
    }

    public sealed class PreviewCounts
    {
        public int Lines { get; set; }
        public decimal Total { get; set; }
    }
}
