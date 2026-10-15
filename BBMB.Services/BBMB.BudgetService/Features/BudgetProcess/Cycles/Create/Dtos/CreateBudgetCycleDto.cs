namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.Create.Dtos
{
    /// <summary>Body of <c>POST /api/budget-cycles</c>; next_fin_year is current + 1 (DDD T-14 CHECK).</summary>
    public sealed class CreateBudgetCycleDto
    {
        public int CurrentFinYear { get; set; }
        public bool IsFirstCycle { get; set; }
    }
}
