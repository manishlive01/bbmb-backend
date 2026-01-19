namespace BBMB.BudgetService.Features.Designations.Update.Dtos
{
    public record UpdateDesignationDto
    {
        public required string DesignationDesc { get; init; }
        public required string UserCode { get; init; }
    }
}
