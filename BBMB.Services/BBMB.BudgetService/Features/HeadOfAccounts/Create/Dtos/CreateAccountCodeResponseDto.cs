namespace BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos
{
    /// <summary>F-01 result: the new code, offered for selection at once (BUD-046).</summary>
    public sealed record CreateAccountCodeResponseDto(long AccountCodeId, string CompleteCode);
}
