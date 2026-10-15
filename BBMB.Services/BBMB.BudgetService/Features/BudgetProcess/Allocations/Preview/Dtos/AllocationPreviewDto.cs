namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Preview.Dtos
{
    /// <summary>
    /// Design §8.6 allocations/preview (BUD-189): what the allocation of a body would activate, shown before
    /// the Chief Engineer confirms. The counts are the DB-20 counts checked again at allocation.
    /// </summary>
    public sealed class AllocationPreviewDto
    {
        public string Body { get; set; } = string.Empty;
        public int LinesToActivate { get; set; }
        public decimal TotalToActivate { get; set; }
        public List<int> WorksToReceiveCode { get; set; } = [];
        public List<AllocationCategoryPreviewDto> ByCategory { get; set; } = [];
        /// <summary>On refusal (M-19): proposals not at the body's placement state.</summary>
        public List<OutstandingProposalDto> Outstanding { get; set; } = [];
    }

    public sealed record AllocationCategoryPreviewDto(string Category, int LinesToActivate, decimal TotalToActivate);

    public sealed record OutstandingProposalDto(long ProposalId, long OriginOfficeId, string StateCode);
}
