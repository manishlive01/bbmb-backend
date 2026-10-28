namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Save.Dtos
{
    /// <summary>Design §8.6 PUT /api/provisions/{provisionId}/estimate-allocations body (F-29).</summary>
    public sealed class SaveEstimateAllocationDto
    {
        public List<EstimateAllocationLineDto> Lines { get; set; } = [];
    }

    public sealed record EstimateAllocationLineDto(long TechnicalSanctionId, decimal Amount);
}
