namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos
{
    /// <summary>One T-14 cycle with its status (design §8.2).</summary>
    public sealed class BudgetCycleResponseDto
    {
        public long CycleId { get; set; }
        public int CurrentFinYear { get; set; }
        public int NextFinYear { get; set; }
        public bool IsFirstCycle { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
