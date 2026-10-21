namespace BBMB.BudgetService.Features.BudgetProcess.Freezes.Freeze.Dtos
{
    /// <summary>Design §8.6 POST freeze body.</summary>
    public sealed class FreezeRequestDto
    {
        public string FaCaoApprovalRef { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }

    /// <summary>On refusal (M-19): the proposals of the cycle not at S-15 (BUD-193).</summary>
    public sealed class FreezeRefusalDto
    {
        public List<OutstandingFreezeDto> Outstanding { get; set; } = [];
    }

    public sealed record OutstandingFreezeDto(long ProposalId, long OriginOfficeId, string StateCode);
}
