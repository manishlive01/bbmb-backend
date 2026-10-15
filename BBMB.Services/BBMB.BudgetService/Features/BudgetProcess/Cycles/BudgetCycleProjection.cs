using System.Linq.Expressions;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles
{
    internal static class BudgetCycleProjection
    {
        public static readonly Expression<Func<BudgetCycle, BudgetCycleResponseDto>> ToDto = c => new BudgetCycleResponseDto
        {
            CycleId = c.CycleId, CurrentFinYear = c.CurrentFinYear, NextFinYear = c.NextFinYear,
            IsFirstCycle = c.IsFirstCycle, Status = c.Status,
        };
    }
}
