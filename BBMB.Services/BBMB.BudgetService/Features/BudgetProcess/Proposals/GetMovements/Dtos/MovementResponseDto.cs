namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements.Dtos
{
    /// <summary>One T-33 movement with its offices and officer (BUD-238). The client IP is held for audit, not shown.</summary>
    public sealed class MovementResponseDto
    {
        public long MovementId { get; set; }
        public string FromState { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string ToState { get; set; } = string.Empty;
        public long FromOfficeId { get; set; }
        public string? FromOfficeName { get; set; }
        public long ToOfficeId { get; set; }
        public string? ToOfficeName { get; set; }
        public string? Remark { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string? OnBehalfOfPost { get; set; }
        public Guid OfficerId { get; set; }
        public Guid ChargeId { get; set; }
        public DateTimeOffset MovedAt { get; set; }
    }
}
