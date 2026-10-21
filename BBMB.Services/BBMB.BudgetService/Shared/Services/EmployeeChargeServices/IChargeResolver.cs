using BBMB.BudgetService.Shared.ServiceDTOs;
namespace BBMB.BudgetService.Shared.Services.EmployeeChargeServices
{
    public interface IChargeResolver
    {
        /// <summary>Project-agnostic; first current charge. Kept for existing non-budget callers.</summary>
        Task<UserChargeContext?> GetCurrentChargeAsync(CancellationToken ct);

        /// <summary>
        /// The one current charge in the Budget project (design §4.3, D-12): never picks one of several;
        /// <c>X-Budget-Charge</c> selects when more than one remains.
        /// </summary>
        Task<BudgetChargeResult> GetCurrentBudgetChargeAsync(CancellationToken ct);
    }
}
