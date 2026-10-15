namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory.Dtos
{
    /// <summary>
    /// Design §8.3 FinaliseCategory result. On success <see cref="Finalised"/> lists the proposals moved to S-11;
    /// on refusal (M-19) the lists say why nothing moved (BUD-134, BUD-302).
    /// </summary>
    public sealed class FinaliseResponseDto
    {
        public List<long> Finalised { get; set; } = [];
        /// <summary>Proposals of the category not at S-10 or S-11.</summary>
        public List<OutstandingProposalDto> Outstanding { get; set; } = [];
        /// <summary>Offices of the expected set with no proposal in the category (BUD-134).</summary>
        public List<long> MissingOffices { get; set; } = [];
        /// <summary>Head-wise: (CE, head) rows with demand and no recommendation (BUD-302).</summary>
        public List<MissingRecommendationDto> MissingRecommendations { get; set; } = [];
    }

    public sealed record OutstandingProposalDto(long ProposalId, long OriginOfficeId, string StateCode);

    public sealed record MissingRecommendationDto(long CeOfficeId, long AccountCodeId);
}
