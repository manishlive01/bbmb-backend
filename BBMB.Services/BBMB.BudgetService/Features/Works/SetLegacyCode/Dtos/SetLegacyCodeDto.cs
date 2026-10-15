namespace BBMB.BudgetService.Features.Works.SetLegacyCode.Dtos
{
    /// <summary>
    /// Request body of <c>PUT /api/works/{id}/legacy-code</c> (design §8.6, F-28).
    /// Carries only the seven-digit legacy code; the origin, occasion and timestamp are set server-side.
    /// </summary>
    public sealed class SetLegacyCodeDto
    {
        public string LegacyCode { get; set; } = string.Empty;
    }
}
