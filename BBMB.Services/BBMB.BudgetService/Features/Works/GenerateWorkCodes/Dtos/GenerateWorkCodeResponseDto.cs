namespace BBMB.BudgetService.Features.Works.GenerateWorkCodes.Dtos
{
    public class GenerateWorkCodeResponseDto
    {
        public string WorkCode { get; set; } = string.Empty;
        public int DivisionCode { get; set; }
        public int SequenceNumber { get; set; }

    }
}
