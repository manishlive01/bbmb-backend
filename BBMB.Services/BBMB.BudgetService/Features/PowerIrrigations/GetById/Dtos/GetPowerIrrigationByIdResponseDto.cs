namespace BBMB.BudgetService.Features.PowerIrrigations.GetById.Dtos
{
    public class GetPowerIrrigationByIdResponseDto
    {
        public string IrrPwrCode { get; set; } = string.Empty;
        public string IrrPwrDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
