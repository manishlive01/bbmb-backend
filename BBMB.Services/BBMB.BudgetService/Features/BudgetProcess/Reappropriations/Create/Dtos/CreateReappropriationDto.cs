namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Create.Dtos
{
    /// <summary>Design §8.7 POST /api/reappropriations body (F-21).</summary>
    public sealed class CreateReappropriationDto
    {
        public int FinYear { get; set; }
        /// <summary>Exactly one of these two destinations (an existing provision or a new work).</summary>
        public long? DestinationProvisionId { get; set; }
        public int? DestinationNewWorkId { get; set; }
        public decimal AdditionalAmount { get; set; }
        public string EofficeOrderRef { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
