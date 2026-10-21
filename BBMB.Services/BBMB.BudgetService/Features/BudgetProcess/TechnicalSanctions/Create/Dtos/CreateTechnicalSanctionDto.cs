namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create.Dtos
{
    /// <summary>
    /// Body of <c>POST /api/technical-sanctions</c> (F-06, design §8.4). AA tab: <see cref="AdminApprovalId"/> only, the
    /// work and head come from the approval (BUD-069). AMR tab: an existing AMR <see cref="WorkId"/> or a
    /// <see cref="NewAmrWork"/> (entered_via TS_ENTRY). The serial is allotted.
    /// </summary>
    public sealed class CreateTechnicalSanctionDto
    {
        public string Tab { get; set; } = string.Empty;
        public long? AdminApprovalId { get; set; }
        public int? WorkId { get; set; }
        public NewAmrWorkDto? NewAmrWork { get; set; }
        public string NameOfEstimate { get; set; } = string.Empty;
        public string TsNumber { get; set; } = string.Empty;
        public DateOnly TsDate { get; set; }
        public decimal TsAmount { get; set; }
        /// <summary>The scanned copy (EVIDENCE, uploaded by the actor). Required (M-07, VR-10).</summary>
        public long? TsCopyAttachmentId { get; set; }
    }

    public sealed class NewAmrWorkDto
    {
        public string NameOfWork { get; set; } = string.Empty;
        public long AccountCodeId { get; set; }
    }
}
