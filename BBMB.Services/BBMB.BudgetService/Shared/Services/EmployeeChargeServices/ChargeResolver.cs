using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace BBMB.BudgetService.Shared.Services.EmployeeChargeServices
{
    public class ChargeResolver : IChargeResolver
    {
        /// <summary>Configuration key of the Budget project code in employee_charge.project_code (OI-45).</summary>
        public const string ProjectCodeKey = "BudgetProcess:ProjectCode";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmployeeContext _context;
        private readonly IConfiguration _configuration;
        public ChargeResolver(
            IHttpContextAccessor httpContextAccessor,
            EmployeeContext context,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
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
            // D-12: is_charge_expire is set lazily, so a charge past charge_expire_time is excluded by time
            // as well (charge_expire_time is timestamptz; DateTime.UtcNow compares the same instant).
            var now = DateTime.UtcNow;
            var charge = await _context.EmployeeCharges
                .AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employee.Id &&
                    x.IsActive &&
                    !x.IsChargeExpire &&
                    (x.ChargeExpireTime == null || x.ChargeExpireTime > now))
                .Select(x => new UserChargeContext
                {
                    UserId = userId,
                    EmployeeId = x.EmployeeId,
                    ChargeId = x.Id,
                    ProjectCode = x.ProjectCode,
                    CEId = x.CEId,
                    CRId = x.CRId,
                    DivisionId = x.DivisionId
                })
                .FirstOrDefaultAsync(ct);
            return charge;
        }

        /// <summary>
        /// Design §4.3 "Charge selection" (D-12): every current charge of the employee in the Budget project
        /// (<c>BudgetProcess:ProjectCode</c>); zero → None; one → it; several → the one named by
        /// <c>X-Budget-Charge</c>, else Ambiguous with the ids. A header naming a charge not held → None.
        /// An unset project code fails closed (None).
        /// </summary>
        public async Task<BudgetChargeResult> GetCurrentBudgetChargeAsync(CancellationToken ct)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return BudgetChargeResult.None;

            var projectCode = _configuration[ProjectCodeKey];
            if (string.IsNullOrWhiteSpace(projectCode))
                return BudgetChargeResult.None;

            if (!Guid.TryParse(JwtClaimHelper.GetUserId(httpContext.User), out var userId))
                return BudgetChargeResult.None;

            var employeeId = await _context.Employees
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(ct);
            if (employeeId == null)
                return BudgetChargeResult.None;

            var now = DateTime.UtcNow;
            var charges = await _context.EmployeeCharges
                .AsNoTracking()
                .Where(x =>
                    x.EmployeeId == employeeId &&
                    x.ProjectCode == projectCode &&
                    x.IsActive &&
                    !x.IsChargeExpire &&
                    (x.ChargeExpireTime == null || x.ChargeExpireTime > now))
                .OrderBy(x => x.ChargeTime)
                .ThenBy(x => x.Id)
                .Select(x => new UserChargeContext
                {
                    UserId = userId,
                    EmployeeId = x.EmployeeId,
                    ChargeId = x.Id,
                    ProjectCode = x.ProjectCode,
                    CEId = x.CEId,
                    CRId = x.CRId,
                    DivisionId = x.DivisionId
                })
                .ToListAsync(ct);

            var header = httpContext.Request.Headers[BudgetChargeResult.HeaderName].ToString();
            if (!string.IsNullOrWhiteSpace(header))
            {
                // A named charge must be one of the employee's current Budget charges.
                if (!Guid.TryParse(header, out var named))
                    return BudgetChargeResult.None;
                var chosen = charges.FirstOrDefault(c => c.ChargeId == named);
                return chosen is null ? BudgetChargeResult.None : BudgetChargeResult.Single(chosen);
            }

            return charges.Count switch
            {
                0 => BudgetChargeResult.None,
                1 => BudgetChargeResult.Single(charges[0]),
                _ => BudgetChargeResult.Ambiguous(charges.Select(c => c.ChargeId).ToList()),
            };
        }
    }
}
