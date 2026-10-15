namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.PlaceBeforeBsc.Dtos
{
    /// <summary>
    /// Result of a cycle-level placement (design §8.6 place-before-bsc / place-before-full-board). On success
    /// <see cref="Moved"/> lists the proposals transitioned; on refusal (M-19) the lists say why nothing moved:
    /// proposals still at a pre-placement state (<see cref="Outstanding"/>) and expected offices with no
    /// proposal (<see cref="MissingOffices"/>, BUD-134).
    /// </summary>
    public sealed class BulkTransitionResponseDto
    {
        public List<long> Moved { get; set; } = [];
        public List<OutstandingProposalDto> Outstanding { get; set; } = [];
        public List<long> MissingOffices { get; set; } = [];
    }

    public sealed record OutstandingProposalDto(long ProposalId, long OriginOfficeId, string StateCode);
}
