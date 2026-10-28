namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Reads
{
    /// <summary>Design §8.6 GET unforeseen-requests (BUD-293): a request with the balance available on each provision drawn upon.</summary>
    public sealed class UnforeseenRequestDto
    {
        public long UnforeseenRequestId { get; set; }
        public int WorkId { get; set; }
        public string? WorkCode { get; set; }
        public long DivisionOfficeId { get; set; }
        public string Status { get; set; } = string.Empty;
        public long HolderOfficeId { get; set; }
        public long? CurrentProvisionId { get; set; }
        public decimal? AmountCurrent { get; set; }
        public decimal? CurrentBalanceAvailable { get; set; }
        public long? NextProvisionId { get; set; }
        public decimal? AmountNext { get; set; }
        public decimal? NextBalanceAvailable { get; set; }
    }
}
