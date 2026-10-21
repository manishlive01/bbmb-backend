namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals.Dtos
{
    /// <summary>
    /// Body of <c>POST /api/budget-cycles/{cycleId}/open-proposals</c> (design §8.2). <see cref="OfficeIds"/>
    /// names the DIVISION / CIRCLE / CE offices that frame a head-wise demand; WORKS ignores it.
    /// </summary>
    public sealed class OpenProposalsDto
    {
        public List<string> Categories { get; set; } = [];
        public List<long>? OfficeIds { get; set; }
    }

    public sealed class OpenProposalsResponseDto
    {
        public List<OpenedProposalDto> Created { get; set; } = [];
        /// <summary>Proposals of the expected set that already existed and were left as they are.</summary>
        public int ExistingCount { get; set; }
    }

    public sealed record OpenedProposalDto(long ProposalId, string Category, long OfficeId);
}
