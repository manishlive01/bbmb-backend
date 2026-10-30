using BBMB.BudgetService.Authorization.Handlers;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Shared.Services.EmployeeChargeServices
{
    public class ChargeResolver : IChargeResolver
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EmployeeContext _context;
        private readonly IBudgetConfigProvider _config;
        private readonly IBudgetOfficeScope _officeScope;
        public ChargeResolver(
            IHttpContextAccessor httpContextAccessor,
            EmployeeContext context,
            IBudgetConfigProvider config,
            IBudgetOfficeScope officeScope)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _config = config;
            _officeScope = officeScope;
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
        /// Resolves the single active Budget charge named by <c>X-Charge-Id</c>. The query scopes the ID
        /// to the authenticated user, active Budget project, and expiry state, then validates the acting
        /// role's office scope. Missing or invalid selections fail closed.
        /// </summary>
        public async Task<BudgetChargeResult> GetCurrentBudgetChargeAsync(CancellationToken ct)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return BudgetChargeResult.None;

            var snapshot = await _config.GetAsync(ct);
            var projectCode = snapshot.ProjectCode;
            if (string.IsNullOrWhiteSpace(projectCode))
                return BudgetChargeResult.None;

            var actingRole = BudgetActingRole.Get(httpContext);
            if (string.IsNullOrWhiteSpace(actingRole))
                return BudgetChargeResult.None;

            if (!Guid.TryParse(JwtClaimHelper.GetUserId(httpContext.User), out var userId))
                return BudgetChargeResult.None;

            var header = httpContext.Request.Headers[BudgetChargeResult.HeaderName].ToString();
            if (!Guid.TryParse(header, out var selectedChargeId))
                return BudgetChargeResult.None;

            var now = DateTime.UtcNow;
            var selectedCharge = await _context.EmployeeCharges
                .AsNoTracking()
                .Where(x =>
                    x.Id == selectedChargeId &&
                    _context.Employees.Any(employee =>
                        employee.Id == x.EmployeeId && employee.UserId == userId) &&
                    x.ProjectCode == projectCode &&
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

            if (selectedCharge is null ||
                await _officeScope.ResolveActorOfficeAsync(selectedCharge, actingRole, ct) is null)
            {
                return BudgetChargeResult.None;
            }

            return BudgetChargeResult.Single(selectedCharge);
        }
    }
}
