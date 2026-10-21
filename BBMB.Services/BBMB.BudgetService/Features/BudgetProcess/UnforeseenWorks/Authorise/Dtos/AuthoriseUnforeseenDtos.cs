namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Authorise.Dtos
{
    /// <summary>Design §8.6 POST /api/unforeseen-requests/{id}/authorise body.</summary>
    public sealed class AuthoriseUnforeseenDto
    {
        public string Reasons { get; set; } = string.Empty;
    }

    /// <summary>Success: the work code generated in the same transaction (BUD-298).</summary>
    public sealed class AuthoriseUnforeseenResponseDto
    {
        public string WorkCode { get; set; } = string.Empty;
    }
}
