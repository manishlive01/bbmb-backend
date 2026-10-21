namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos
{
    /// <summary>The proposal's state after a move (design §8.1).</summary>
    public sealed class ProposalStateResponseDto
    {
        public long ProposalId { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public long HolderOfficeId { get; set; }
        public string? HolderOfficeName { get; set; }

        /// <summary>Message ids of advisory warnings (M-26 from P3).</summary>
        public List<string> Warnings { get; set; } = [];
    }
}
