using BBMB.BudgetService.Shared.ServiceDTOs;
namespace BBMB.BudgetService.Shared.Services.EmployeeChargeServices
{
    public interface IChargeResolver
    {
        /// <summary>Project-agnostic; first current charge. Kept for existing non-budget callers.</summary>
        Task<UserChargeContext?> GetCurrentChargeAsync(CancellationToken ct);

        /// <summary>
        /// Resolves the authenticated user's current Budget charge. When the employee holds exactly one it is
        /// selected automatically; when several are held, <c>X-Charge-Id</c> must name one
        /// (<see cref="BudgetChargeResult.Ambiguous"/> otherwise). Fails closed with None.
        /// </summary>
        Task<BudgetChargeResult> GetCurrentBudgetChargeAsync(CancellationToken ct);
    }
}
