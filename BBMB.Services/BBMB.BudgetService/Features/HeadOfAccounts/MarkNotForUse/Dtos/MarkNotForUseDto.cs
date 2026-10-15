namespace BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse.Dtos
{
    /// <summary>Body of <c>POST /api/head-of-accounts/{id}/not-for-use</c>: the e-Office reference of the approval of the correction (BUD-308).</summary>
    public sealed class MarkNotForUseDto
    {
        public string EofficeRef { get; set; } = string.Empty;
    }
}
