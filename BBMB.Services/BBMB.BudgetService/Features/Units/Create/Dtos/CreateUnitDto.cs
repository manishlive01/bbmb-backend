namespace BBMB.BudgetService.Features.Units.Create.Dtos
{
    public class CreateUnitDto
    {
        public string UnitCode { get; set; } = string.Empty;
        public string UnitDesc { get; set; } = string.Empty;
        public string ProjCode { get; set; } = string.Empty;
        public string IrrPwrCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = "123";
    }
}
