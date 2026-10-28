using BBMB.BudgetService.Shared.ResponseDTOs;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal.Dtos
{
    /// <summary>
    /// Design §8.3 <c>GET …/demand-lines</c>: one page of V-04 rows with the column headings of the proposal's
    /// cycle (BUD-305). Carried in <c>Response&lt;T&gt;</c> rather than a bare paged list so the 404 of the
    /// scope rule (§4.9) and the headings travel with the page.
    /// </summary>
    public sealed class GetDemandLinesResponseDto
    {
        public long ProposalId { get; set; }
        public string Category { get; set; } = string.Empty;
        /// <summary>
        /// Heading of each figure field, naming the financial year it relates to and "receipts" for RECEIPTS
        /// (BUD-305): keys rbeAmount, obeNextAmount, openingObeCurrent, openingRbePrevious, expenditureToDate,
        /// expenditurePreviousYear.
        /// </summary>
        public Dictionary<string, string> Headings { get; set; } = [];
        public List<DemandLineRowDto> Lines { get; set; } = [];
        public PaginationMetadata Pagination { get; set; } = new();
    }

    /// <summary>One V-04 row (Table 3 columns 1–14 / Table 4 columns 1–11 as entered and joined).</summary>
    public sealed class DemandLineRowDto
    {
        public long DemandLineId { get; set; }
        public int SerialNo { get; set; }
        public string LineType { get; set; } = string.Empty;
        public int? WorkId { get; set; }
        public string? WorkDesc { get; set; }
        public string? WorkClass { get; set; }
        public int? PrecedingAmrWorkId { get; set; }
        public long? AccountCodeId { get; set; }
        public string? CompleteCode { get; set; }
        public string? HeadDesc { get; set; }
        public string? UnforeseenName { get; set; }
        public decimal? RbeAmount { get; set; }
        public decimal? ObeNextAmount { get; set; }
        public string? JustificationText { get; set; }
        /// <summary>
        /// Column 13 supporting file and the CE's certificate / AM&amp;R approval. Returned so the client can send
        /// them back on PUT …/demand-lines/{lineId}: that PUT replaces the line's values, and a null clears them.
        /// </summary>
        public long? JustificationAttachmentId { get; set; }
        public long? CeCertificateAttachmentId { get; set; }
        public long? CeAmrApprovalAttachmentId { get; set; }
        public decimal? OpeningObeCurrent { get; set; }
        public decimal? OpeningRbePrevious { get; set; }
        public decimal? ExpenditureToDate { get; set; }
        public decimal? ExpenditurePreviousYear { get; set; }
        public bool PctCheckNotApplied { get; set; }
        public bool FloorCheckUnchecked { get; set; }
        public bool AddedOnReturn { get; set; }
        public bool IsDeleted { get; set; }
        /// <summary>The latest figure at each level office (T-31, BUD-115).</summary>
        public List<LevelFigureDto> LevelFigures { get; set; } = [];
    }

    public sealed record LevelFigureDto(long LevelOfficeId, string Figure, decimal? Value);
}
