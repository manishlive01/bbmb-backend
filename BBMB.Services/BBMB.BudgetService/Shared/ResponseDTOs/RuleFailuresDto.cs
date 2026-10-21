namespace BBMB.BudgetService.Shared.ResponseDTOs
{
    /// <summary>
    /// One rule failure or warning (design §4.2, §4.9; BUD-096, BUD-100). Names the line and the field
    /// concerned so the screen can point at it; never carries a table or constraint name (BUD-012).
    /// </summary>
    public sealed record RuleFailureItemDto(
        long? DemandLineId,
        int? SerialNo,
        string Field,
        string MessageId,
        string Message);

    /// <summary>Body of a Submit-type refusal: <c>Response&lt;RuleFailuresDto&gt;</c>.</summary>
    public sealed class RuleFailuresDto
    {
        public List<RuleFailureItemDto> Failures { get; set; } = [];
        public List<RuleFailureItemDto> Warnings { get; set; } = [];
    }
}
