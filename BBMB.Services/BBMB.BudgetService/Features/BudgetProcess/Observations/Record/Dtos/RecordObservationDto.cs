namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Record.Dtos
{
    /// <summary>Design §8.3 <c>POST …/observations</c> body (T-32).</summary>
    public sealed class RecordObservationDto
    {
        /// <summary>A line of the same proposal; null = a foot remark on the proposal (BUD-128).</summary>
        public long? DemandLineId { get; set; }
        public string ObservationText { get; set; } = string.Empty;
    }
}
