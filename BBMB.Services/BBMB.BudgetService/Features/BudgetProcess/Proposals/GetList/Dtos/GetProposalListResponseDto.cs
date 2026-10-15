namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList.Dtos
{
    public sealed class GetProposalListResponseDto
    {
        public long ProposalId { get; set; }
        public long CycleId { get; set; }
        public string Category { get; set; } = string.Empty;
        public long OriginOfficeId { get; set; }
        public string? OriginOfficeName { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public long HolderOfficeId { get; set; }
        public string? HolderOfficeName { get; set; }
        public DateTimeOffset? FirstSubmittedAt { get; set; }
    }
}
