namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos
{
    /// <summary>A sanction with every version (BUD-271) and its estimate code once Financial Accounting has recorded it (BUD-272).</summary>
    public sealed class TechnicalSanctionDto
    {
        public long TechnicalSanctionId { get; set; }
        public int WorkId { get; set; }
        public string Tab { get; set; } = string.Empty;
        public long? AdminApprovalId { get; set; }
        public int SerialNo { get; set; }
        public short CurrentVersionNo { get; set; }
        public string? EstimateCode { get; set; }
        public DateTimeOffset? EstimateCodeAt { get; set; }
        public List<TechnicalSanctionVersionDto> Versions { get; set; } = [];
    }

    public sealed class TechnicalSanctionVersionDto
    {
        public short VersionNo { get; set; }
        public string NameOfEstimate { get; set; } = string.Empty;
        public string TsNumber { get; set; } = string.Empty;
        public DateOnly TsDate { get; set; }
        public decimal TsAmount { get; set; }
        public long TsCopyAttachmentId { get; set; }
        public string TsCopyUrl => $"/api/budget-attachments/{TsCopyAttachmentId}";
        public DateTimeOffset RecordedAt { get; set; }
    }
}
