namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetQueryFields.Dtos
{
    /// <summary>Design §8.8 (BUD-220): a selectable field by business name, with its kind and description.</summary>
    public sealed record QueryFieldDto(string Name, string Kind, string Description);
}
