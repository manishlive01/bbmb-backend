namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Reads
{
    /// <summary>Design §8.7 GET reappropriations / {id} (BUD-207): an entry with its sources and before/after figures.</summary>
    public sealed class ReappropriationDto
    {
        public long ReappropriationId { get; set; }
        public int FinYear { get; set; }
        public int SerialNo { get; set; }
        public string Status { get; set; } = string.Empty;
        public long? DestinationProvisionId { get; set; }
        public int? DestinationNewWorkId { get; set; }
        public decimal AdditionalAmount { get; set; }
        public string EofficeOrderRef { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public long? OrderAttachmentId { get; set; }
        public List<ReappropriationSourceDto> Sources { get; set; } = [];
    }

    /// <summary>A surrender source with the provision's available figure now (after any movement already written).</summary>
    public sealed record ReappropriationSourceDto(
        long ReappropriationSourceId, long ProvisionId, decimal Amount, bool IsRemoved, decimal? ProvisionAvailable);

    /// <summary>Design §8.7 GET new-work-candidates: works eligible as a new-work destination (no code, no provision).</summary>
    public sealed record NewWorkCandidateDto(int WorkId, string WorkDesc, int DivisionId);

    /// <summary>Design §8.7 GET register (V-10, F-22): one register row (a re-appropriation or an unforeseen authorisation).</summary>
    public sealed class ReappropriationRegisterDto
    {
        public string EntryKind { get; set; } = string.Empty;
        public long EntryId { get; set; }
        public int FinYear { get; set; }
        public int? SerialNo { get; set; }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? EofficeRef { get; set; }
        public long OfficeId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
