namespace BBMB.BudgetService.Features.Units.GetByProject.Dtos
{
    public class GetUnitsByProjectResponseDto
    {
        public string UnitCode { get; set; } = string.Empty;
        public string UnitDesc { get; set; } = string.Empty;
        public string ProjCode { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string IrrPwrCode { get; set; } = string.Empty;
        public string PowerIrrigationDescription { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
