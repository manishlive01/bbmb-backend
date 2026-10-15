namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos
{
    /// <summary>
    /// Body of a transition that carries a remark (forward, forward-to-finance, send-back, reopen). Whether the
    /// remark is required is T-16 <c>remark_required</c>, checked by the service (M-12, BUD-120).
    /// </summary>
    public sealed class TransitionRemarkDto
    {
        public string? Remark { get; set; }
    }
}
