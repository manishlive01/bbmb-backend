namespace BBMB.BudgetService.Features.ObjectClassifications.Update.Dtos
{
    public record UpdateObjectClassificationDto
    {
        public required string ObjectAccDesc { get; init; }
        public required string UserCode { get; init; }
    }
}
