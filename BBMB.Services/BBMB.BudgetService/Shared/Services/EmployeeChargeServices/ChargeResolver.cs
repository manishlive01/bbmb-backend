using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BBMB.BudgetService.Shared.Services.EmployeeChargeServices
{
    public class ChargeResolver : IChargeResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmployeeContext _context;

        public ChargeResolver(
            IHttpContextAccessor httpContextAccessor,
            EmployeeContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<UserChargeContext?> GetCurrentChargeAsync(CancellationToken ct)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
                return null;

            // ✅ Use JwtClaimHelper method
            var userIdClaim = JwtClaimHelper.GetUserId(httpContext.User);

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            var userId = Guid.Parse(userIdClaim);

            // ================= GET EMPLOYEE =================

            var employee = await _context.Employees
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new { x.Id })
                .FirstOrDefaultAsync(ct);

            if (employee == null)
                return null;

            // ================= GET ACTIVE CHARGE =================

            var charge = await _context.EmployeeCharges
                .AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employee.Id &&
                    x.IsActive &&
                    !x.IsChargeExpire)
                .Select(x => new UserChargeContext
                {
                    UserId = userId,
                    EmployeeId = x.EmployeeId,
                    ProjectCode = x.ProjectCode,
                    CEId = x.CEId,
                    CRId = x.CRId,
                    DivisionId = x.DivisionId
                })
                .FirstOrDefaultAsync(ct);

            return charge;
        }
    }
}