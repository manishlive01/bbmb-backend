using BBMB.BudgetService.Shared.ResponseDTOs;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos
{
    /// <summary>
    /// Result of a demand-line Save (design §8.3 Add / Update): the line, its serial (Table 3 column 1) and the
    /// warnings of the Save moment (§6.4: the ceilings warn on Save, M-32 where no percentage was computed).
    /// </summary>
    public sealed record SaveDemandLineResponseDto(long DemandLineId, int SerialNo, IReadOnlyList<RuleFailureItemDto> Warnings);
}
