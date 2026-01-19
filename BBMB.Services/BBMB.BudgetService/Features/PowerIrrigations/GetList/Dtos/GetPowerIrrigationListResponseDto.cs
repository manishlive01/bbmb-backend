namespace BBMB.BudgetService.Features.PowerIrrigations.GetList.Dtos
{
    public class GetPowerIrrigationListResponseDto
    {
        public string IrrPwrCode { get; set; } = string.Empty;
        public string IrrPwrDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
