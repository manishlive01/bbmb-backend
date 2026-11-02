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
        /// Resolves the acting user's current Budget charge (design §4.3 "Charge selection", D-12):
        /// <list type="number">
        /// <item>the current Budget charges of the authenticated user (active, not expired, matching the
        /// configured project code) are loaded;</item>
        /// <item>exactly one → it is selected without needing a header;</item>
        /// <item>several → <c>X-Charge-Id</c> must name one of them; no header is <see cref="BudgetChargeOutcome.Ambiguous"/>
        /// (the caller answers M-20 with the ids, never picks); a header naming a charge not held is
        /// <see cref="BudgetChargeOutcome.None"/>;</item>
        /// <item>the selected charge must resolve to an office for the acting role.</item>
        /// </list>
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

            // D-12: current = active, not flagged expired, and not past charge_expire_time.
            var now = DateTime.UtcNow;
            var current = await _context.EmployeeCharges
                .AsNoTracking()
                .Where(x =>
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
                .ToListAsync(ct);

            if (current.Count == 0)
                return BudgetChargeResult.None;

            UserChargeContext selected;
            var header = httpContext.Request.Headers[BudgetChargeResult.HeaderName].ToString();
            if (!string.IsNullOrWhiteSpace(header))
            {
                if (!Guid.TryParse(header, out var selectedChargeId))
                    return BudgetChargeResult.None;

                var named = current.FirstOrDefault(x => x.ChargeId == selectedChargeId);
                if (named is null)
                    return BudgetChargeResult.None;
                selected = named;
            }
            else if (current.Count == 1)
            {
                // One current Budget charge needs no selection: this is the common DDO case.
                selected = current[0];
            }
            else
            {
                return BudgetChargeResult.Ambiguous(current.Select(x => x.ChargeId).ToList());
            }

            if (await _officeScope.ResolveActorOfficeAsync(selected, actingRole, ct) is null)
                return BudgetChargeResult.None;

            return BudgetChargeResult.Single(selected);
        }
    }
}
