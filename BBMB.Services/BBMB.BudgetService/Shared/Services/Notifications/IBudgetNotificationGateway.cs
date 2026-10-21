namespace BBMB.BudgetService.Shared.Services.Notifications
{
    /// <summary>
    /// Boundary to the SMS / e-mail gateway (INT-004, design §4.11). Called after commit; a failure never undoes
    /// the action (design §10: Warning, normal success).
    /// </summary>
    public interface IBudgetNotificationGateway
    {
        Task NotifyMovementAsync(long proposalId, long movementId, CancellationToken ct = default);
    }

    /// <summary>No gateway is integrated (§15 out of scope): nothing is sent.</summary>
    public sealed class NoOpBudgetNotificationGateway : IBudgetNotificationGateway
    {
        public Task NotifyMovementAsync(long proposalId, long movementId, CancellationToken ct = default) => Task.CompletedTask;
    }
}
