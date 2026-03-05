using BBMB.BudgetService.Shared.ServiceDTOs;

namespace BBMB.BudgetService.Shared.Services.EmployeeChargeServices
{
    public interface IChargeResolver
    {
        Task<UserChargeContext?> GetCurrentChargeAsync(CancellationToken ct);
    }
}
