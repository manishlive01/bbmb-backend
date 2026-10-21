namespace BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal.Dtos
{
    /// <summary>One T-32 observation with its level, officer, reply and the movement that returned it (BUD-121).</summary>
    public sealed class ObservationResponseDto
    {
        public long ObservationId { get; set; }
        public long? DemandLineId { get; set; }
        public int? SerialNo { get; set; }
        public long LevelOfficeId { get; set; }
        public string? LevelOfficeName { get; set; }
        public string ObservationText { get; set; } = string.Empty;
        public Guid RecordedBy { get; set; }
        public DateTimeOffset RecordedAt { get; set; }
        public bool SelectedForReturn { get; set; }
        public long? ReturnedByMovementId { get; set; }
        public string? ReplyText { get; set; }
        public Guid? RepliedBy { get; set; }
        public DateTimeOffset? RepliedAt { get; set; }
    }
}
