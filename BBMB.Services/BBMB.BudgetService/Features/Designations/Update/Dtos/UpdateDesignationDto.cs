namespace BBMB.BudgetService.Features.Designations.Update.Dtos
{
    public record UpdateDesignationDto
    {
        public string DesignationCode { get; set; } = string.Empty;
        public required string DesignationDesc { get; init; }
        public string ClassCode { get; set; } = string.Empty;
        public decimal? MinBasic { get; set; }
        public decimal? MaxBasic { get; set; }
        public required string UserCode { get; init; }
    }
}
