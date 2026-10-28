using Auth.Authorization.Handlers;
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
        /// Requirement 5 / design §5. Every current charge of the employee in the Budget project, then those
        /// that resolve to a <c>budget.office</c> of the acting role's office level:
        /// <list type="bullet">
        /// <item>project code comes from the active <c>budget_project_settings</c> row (never <c>IConfiguration</c>,
        /// never the hard-coded "1"); an empty project code or no acting role fails closed (None);</item>
        /// <item>ALL employee rows for the token's user_id are considered — the DB has duplicate rows per user,
        /// so a single <c>FirstOrDefault</c> silently drops charges (the former bug);</item>
        /// <item>candidates are filtered by the acting role's office level (a DDO charge must resolve to a
        /// DIVISION office, etc.), so "one candidate ⇒ use it" is meaningful across levels;</item>
        /// <item>exactly one ⇒ it; several ⇒ the one named by <c>X-Budget-Charge</c>, else Ambiguous with the
        /// ids; a header naming a charge outside the filtered set ⇒ treated as missing (Ambiguous), never
        /// honoured; zero after filtering ⇒ None (fails closed).</item>
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

            // ALL employee rows for this user (duplicate rows per user exist); IN (ids) over every one of them.
            var employeeIds = await _context.Employees
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.Id)
                .ToListAsync(ct);
            if (employeeIds.Count == 0)
                return BudgetChargeResult.None;

            var now = DateTime.UtcNow;
            var charges = await _context.EmployeeCharges
                .AsNoTracking()
                .Where(x =>
                    employeeIds.Contains(x.EmployeeId) &&
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

            // Keep only charges that resolve to an office of the acting role's level (design §5 change 2).
            var candidates = new List<UserChargeContext>(charges.Count);
            foreach (var c in charges)
            {
                if (await _officeScope.ResolveActorOfficeAsync(c, actingRole, ct) is not null)
                    candidates.Add(c);
            }

            var header = httpContext.Request.Headers[BudgetChargeResult.HeaderName].ToString();
            if (!string.IsNullOrWhiteSpace(header))
            {
                // A named charge must be one of the filtered candidate charges, else treated as missing.
                if (!Guid.TryParse(header, out var named))
                    return Missing(candidates);
                var chosen = candidates.FirstOrDefault(c => c.ChargeId == named);
                return chosen is null ? Missing(candidates) : BudgetChargeResult.Single(chosen);
            }

            return candidates.Count switch
            {
                0 => BudgetChargeResult.None,
                1 => BudgetChargeResult.Single(candidates[0]),
                _ => BudgetChargeResult.Ambiguous(candidates.Select(c => c.ChargeId).ToList()),
            };
        }

        /// <summary>An unresolved selection: Ambiguous with the selectable ids when any remain, else None.</summary>
        private static BudgetChargeResult Missing(IReadOnlyList<UserChargeContext> candidates) =>
            candidates.Count == 0
                ? BudgetChargeResult.None
                : BudgetChargeResult.Ambiguous(candidates.Select(c => c.ChargeId).ToList());
    }
}
