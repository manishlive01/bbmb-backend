namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.GetForDivision.Dtos
{
    /// <summary>
    /// Design §8.6 GET estimate-allocations (F-29, BUD-288, BUD-289): one row per work holding a live WORK
    /// provision in the division and year, with its available amount, the estimate codes of its sanctions and
    /// what is allocated to each, the unallocated balance, and whether a critical correction flag stands.
    /// </summary>
    public sealed class EstimateAllocationWorkDto
    {
        public long ProvisionId { get; set; }
        public int WorkId { get; set; }
        public string? WorkCode { get; set; }
        public string WorkDesc { get; set; } = string.Empty;
        public int FinYear { get; set; }
        public decimal Available { get; set; }
        public decimal Allocated { get; set; }
        public decimal UnallocatedBalance { get; set; }
        public bool CriticalFlag { get; set; }
        public List<EstimateAllocationCodeDto> Estimates { get; set; } = [];
    }

    public sealed record EstimateAllocationCodeDto(long TechnicalSanctionId, string EstimateCode, decimal Allocated);
}
