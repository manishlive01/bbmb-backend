namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Return.Dtos
{
    /// <summary>Design §8.3 <c>POST …/return</c> body.</summary>
    public sealed class ReturnProposalDto
    {
        /// <summary>Required by T-16 for every RETURN row (M-12, BUD-120).</summary>
        public string? Remark { get; set; }
        /// <summary>The lines whose observations are returned; null = every line (BUD-106).</summary>
        public List<long>? SelectedDemandLineIds { get; set; }
    }
}
