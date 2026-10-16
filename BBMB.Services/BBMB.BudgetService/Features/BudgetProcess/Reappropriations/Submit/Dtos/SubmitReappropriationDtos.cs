namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Submit.Dtos
{
    /// <summary>Design §8.7 POST /api/reappropriations/{id}/submit body.</summary>
    public sealed class SubmitReappropriationDto
    {
        public long OrderAttachmentId { get; set; }
        /// <summary>The officer confirms the balanced entry (BUD-203).</summary>
        public bool Confirm { get; set; }
    }

    /// <summary>Success: the work code generated for a new-work destination, if any (BUD-284).</summary>
    public sealed class SubmitReappropriationResponseDto
    {
        public string? WorkCodeGenerated { get; set; }
    }
}
