namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById.Dtos
{
    public sealed class GetProposalByIdResponseDto
    {
        public long ProposalId { get; set; }
        public long CycleId { get; set; }
        public string Category { get; set; } = string.Empty;
        public long OriginOfficeId { get; set; }
        public string? OriginOfficeName { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public bool FiguresOpen { get; set; }
        public long HolderOfficeId { get; set; }
        public string? HolderOfficeName { get; set; }
        public DateTimeOffset? FirstSubmittedAt { get; set; }
        /// <summary>The officer holding a live edit lease, else null (design §4.10).</summary>
        public Guid? LeaseHeldBy { get; set; }
        public DateTimeOffset? LeaseExpiresAt { get; set; }
        /// <summary>T-16 actions open to the actor now; empty while T-16 is absent (design §11.4).</summary>
        public IReadOnlyList<string> AvailableActions { get; set; } = [];
    }
}
