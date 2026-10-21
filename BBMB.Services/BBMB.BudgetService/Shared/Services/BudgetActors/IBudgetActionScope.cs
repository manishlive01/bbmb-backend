using Microsoft.EntityFrameworkCore.Storage;

namespace BBMB.BudgetService.Shared.Services.BudgetActors
{
    /// <summary>
    /// Opens the explicit transaction of one business action and, as its first statement, sets the actor
    /// context for that transaction (DDD §7.1, design §4.3). The audit trail is written by the database
    /// triggers from these settings; the application never writes it.
    /// </summary>
    public interface IBudgetActionScope
    {
        /// <param name="actionCode">The business action recorded in T-51 (SUBMIT, WORKS_ENTER …).</param>
        /// <param name="justification">BUD-122 justification, recorded with the figure version.</param>
        /// <param name="noJustification">The user chose "Continue without justification" (BUD-122).</param>
        /// <param name="onBehalfOfPost">The post acted for (AFA for FA &amp; CAO, BUD-037, BUD-127).</param>
        Task<IDbContextTransaction> BeginAsync(
            string actionCode,
            string? justification = null,
            bool noJustification = false,
            string? onBehalfOfPost = null,
            CancellationToken ct = default);
    }
}
