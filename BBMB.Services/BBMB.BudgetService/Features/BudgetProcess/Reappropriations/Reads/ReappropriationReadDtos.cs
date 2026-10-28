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
        // Additive (BE-GAP-02, F-21 columns 2 to 6, BUD-207). Existing callers ignore them.
        /// <summary>
        /// RECORDED for a SUBMITTED entry (before/after read from the T-42 ledger at this entry's movement);
        /// PROJECTED for an OPEN entry (before = the ledger now, after = what submitting would leave);
        /// null for an ABANDONED entry (nothing moved, no figures shown).
        /// </summary>
        public string? FiguresBasis { get; set; }
        /// <summary>WORK, HEAD or UNFORESEEN; WORK for a new-work destination.</summary>
        public string? DestinationHolderType { get; set; }
        /// <summary>Column 3: the work's description (WORK, new work) or the head's description (HEAD).</summary>
        public string? DestinationName { get; set; }
        public string? DestinationWorkCode { get; set; }
        public long? DestinationAccountCodeId { get; set; }
        public string? DestinationAccountCode { get; set; }
        public long? DestinationOfficeId { get; set; }
        public string? DestinationOfficeName { get; set; }
        /// <summary>Column 4: the receiving provision before this entry (nil for a new work).</summary>
        public decimal? DestinationExistingBudget { get; set; }
        /// <summary>Column 6: the receiving provision after this entry (column 4 + column 5).</summary>
        public decimal? DestinationAfter { get; set; }
        /// <summary>The receiving provision's available figure now (V-02); null for a new work not yet provided for.</summary>
        public decimal? DestinationAvailableNow { get; set; }
    }

    /// <summary>
    /// A surrender source with the provision's available figure now (after any movement already written). The
    /// trailing fields are additive (BE-GAP-02, columns 7 to 11, BUD-207): name, code and the before/after figures
    /// on the same basis as <see cref="ReappropriationDto.FiguresBasis"/>. A removed source moves nothing, so its
    /// after equals its before.
    /// </summary>
    public sealed record ReappropriationSourceDto(
        long ReappropriationSourceId, long ProvisionId, decimal Amount, bool IsRemoved, decimal? ProvisionAvailable,
        string? HolderType = null, string? ProvisionName = null, string? WorkCode = null,
        long? AccountCodeId = null, string? AccountCode = null, long? OfficeId = null, string? OfficeName = null,
        decimal? ProvisionBefore = null, decimal? ProvisionAfter = null);

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
