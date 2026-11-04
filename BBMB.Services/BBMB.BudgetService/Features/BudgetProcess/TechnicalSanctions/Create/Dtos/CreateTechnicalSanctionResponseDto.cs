namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create.Dtos
{
    /// <summary>F-06 result (as the F-03 result, <c>CreateAdminApprovalResponseDto</c>).</summary>
    public sealed class CreateTechnicalSanctionResponseDto
    {
        public long TechnicalSanctionId { get; set; }
        public int SerialNo { get; set; }
        public int WorkId { get; set; }
        /// <summary>
        /// AMR tab only: the line added to the current WORKS demand (AM&amp;R analogue of BUD-059), or null when none
        /// was added (no open S-01/S-04 demand, or the work already lined). Always null on the AA tab.
        /// </summary>
        public long? DemandLineId { get; set; }
    }
}
