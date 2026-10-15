namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos
{
    /// <summary>Design §4.10: <c>granted:false, readOnly:true</c> opens the proposal read-only; it is not an error.</summary>
    public sealed record LeaseResponseDto(bool Granted, bool ReadOnly, Guid? HeldBy, DateTimeOffset? ExpiresAt);
}
