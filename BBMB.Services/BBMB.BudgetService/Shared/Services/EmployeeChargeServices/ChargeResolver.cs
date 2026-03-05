using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ServiceDTOs;
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
            var userIdClaim = _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            var userId = Guid.Parse(userIdClaim);

            var charge = await _context.EmployeeCharges
                .Where(x =>
                    x.UserId == userId &&
                    x.IsActive &&
                    !x.IsChargeExpire)
                .Select(x => new UserChargeContext
                {
                    UserId = x.UserId,
                    EmployeeId = x.EmployeeId,
                    ProjectId = x.ProjectId,
                    CECode = x.CECode,
                    CRCode = x.CRCode,
                    DivCode = x.DivCode
                })
                .FirstOrDefaultAsync(ct);

            return charge;
        }
    }
}
