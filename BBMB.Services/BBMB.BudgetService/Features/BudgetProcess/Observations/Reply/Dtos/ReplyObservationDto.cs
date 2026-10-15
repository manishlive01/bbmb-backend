namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Reply.Dtos
{
    /// <summary>Design §8.3 <c>PUT …/observations/{obsId}/reply</c> body.</summary>
    public sealed class ReplyObservationDto
    {
        public string ReplyText { get; set; } = string.Empty;
    }
}
