using BBMB.BudgetService.Shared.ServiceDTOs;
namespace BBMB.BudgetService.Shared.Services.EmployeeChargeServices
{
    public interface IChargeResolver
    {
        /// <summary>Project-agnostic; first current charge. Kept for existing non-budget callers.</summary>
        Task<UserChargeContext?> GetCurrentChargeAsync(CancellationToken ct);

        /// <summary>
        /// Resolves the active Budget charge named by <c>X-Charge-Id</c> for the authenticated user.
        /// </summary>
        Task<BudgetChargeResult> GetCurrentBudgetChargeAsync(CancellationToken ct);
    }
}
