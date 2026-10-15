namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Create.Dtos
{
    /// <summary>Design §8.6 POST /api/unforeseen-requests body (F-30, DB-25).</summary>
    public sealed class CreateUnforeseenRequestDto
    {
        public int WorkId { get; set; }
        /// <summary>The UNFORESEEN provision drawn upon for the current year (of the Division's office).</summary>
        public long? CurrentProvisionId { get; set; }
        public decimal? AmountCurrent { get; set; }
        /// <summary>The UNFORESEEN provision for the coming year, where drawn upon.</summary>
        public long? NextProvisionId { get; set; }
        public decimal? AmountNext { get; set; }
    }
}
