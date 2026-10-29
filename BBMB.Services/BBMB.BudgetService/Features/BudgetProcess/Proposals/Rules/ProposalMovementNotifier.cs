using BBMB.BudgetService.Shared.Services.Notifications;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules
{
    /// <summary>
    /// Sends the after-commit movement notification for <see cref="ProposalTransitionService"/> (design §4.11).
    /// Groups the gateway with the logger that only recorded its failures: a notification failure is logged as
    /// a warning and never undoes or fails the committed move.
    /// </summary>
    public sealed class ProposalMovementNotifier(IBudgetNotificationGateway notifications, ILogger<ProposalMovementNotifier> logger)
    {
        public async Task NotifyAsync(long proposalId, long movementId, CancellationToken ct)
        {
            try
            {
                await notifications.NotifyMovementAsync(proposalId, movementId, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Movement notification failed for proposal {ProposalId}, movement {MovementId}",
                    proposalId, movementId);
            }
        }
    }
}
