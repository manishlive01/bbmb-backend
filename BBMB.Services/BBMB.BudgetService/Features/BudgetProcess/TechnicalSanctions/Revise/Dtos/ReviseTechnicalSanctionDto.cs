namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise.Dtos
{
    /// <summary>Body of <c>POST /api/technical-sanctions/{id}/revisions</c> (design §8.4): the new version's particulars.</summary>
    public sealed class ReviseTechnicalSanctionDto
    {
        public string NameOfEstimate { get; set; } = string.Empty;
        public string TsNumber { get; set; } = string.Empty;
        public DateOnly TsDate { get; set; }
        public decimal TsAmount { get; set; }
        public long? TsCopyAttachmentId { get; set; }
    }
}
