namespace BBMB.BudgetService.Features.Designations.Create.Dtos
{
    public class CreateDesignationDto
    {
        public string DesignationCode { get; set; } = string.Empty;
        public string DesignationDesc { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public decimal? MinBasic { get; set; }
        public decimal? MaxBasic { get; set; }

    }
}
