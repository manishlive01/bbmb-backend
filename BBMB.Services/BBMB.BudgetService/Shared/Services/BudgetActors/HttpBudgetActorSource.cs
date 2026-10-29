using System.Net;
using Auth.Authorization.Handlers;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Shared.Services.EmployeeChargeServices;
using BBMB.BudgetService.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Shared.Services.BudgetActors
{
    /// <summary>
    /// Resolves the actor from the request (design §4.3): charge through
    /// <see cref="IChargeResolver.GetCurrentBudgetChargeAsync"/> only; role from the value
    /// <c>BudgetRightHandler</c> stored in <c>HttpContext.Items</c> (never re-read from the claim or header);
    /// office through <see cref="IBudgetOfficeScope"/>; name from employee.name_english; IP from the
    /// connection, else <see cref="IPAddress.None"/>. Never from the request body. Resolved once per request.
    /// </summary>
    /// <remarks>
    /// Effective-admin branch. <c>BudgetRightHandler</c> already grants an effective-admin acting role every
    /// configured right without a charge (the role is in <see cref="BudgetConfigSnapshot.AdminRoles"/>), but the
    /// operational resolution here demanded a charge of the acting role's office level, so a pure-admin user
    /// with no operational <c>employee_charge</c> resolved to no actor (<c>NO_CHARGE</c> → M-20) and was blocked
    /// from every endpoint that resolves one. When the acting role is an effective admin — read from the same
    /// DB-backed <see cref="BudgetConfigSnapshot.AdminRoles"/> signal the handler uses, never a hard-coded role
    /// name — the actor is resolved without demanding an office-level-matched operational charge:
    /// <list type="bullet">
    /// <item>office: the single active office of the admin role's configured level (FINANCE/SECRETARIAT/IT_CELL
    /// are role-level, not charge-dependent), via <see cref="IBudgetOfficeScope.ResolveActorOfficeAsync"/>;
    /// <c>NO_ADMIN_OFFICE</c> if the role has no office level or no such office;</item>
    /// <item>charge: the audit trigger <c>budget.fn_require_actor</c> requires <c>actor_charge_id</c> to be a
    /// current active charge of the actor employee (DB-28, NOT NULL in <c>budget.audit_log</c>), so a sentinel
    /// id cannot be used. An admin's charge need not match the admin office level; any current active charge of
    /// the employee is accepted by the trigger. A budget-project charge is preferred, else any current charge;
    /// if the admin holds none at all, resolution fails with <c>NO_ADMIN_CHARGE</c> (a specific reason telling
    /// the operator to attach a charge) rather than a misleading M-20 — no new migration, the record shape is
    /// unchanged;</item>
    /// <item>employee id / name: from the token user_id → employee lookup, which exists without any charge.</item>
    /// </list>
    /// A NON-admin acting role keeps today's behaviour exactly: it still resolves through the charge, and
    /// <c>NO_CHARGE</c> / <c>AMBIGUOUS_CHARGE</c> are unchanged.
    /// </remarks>
    public sealed class HttpBudgetActorSource(
        IHttpContextAccessor httpContextAccessor,
        IChargeResolver chargeResolver,
        IBudgetOfficeScope officeScope,
        IBudgetConfigProvider configProvider,
        EmployeeContext employeeContext,
        ILogger<HttpBudgetActorSource> logger) : IBudgetActorSource
    {
        private const string RequestIdItemKey = "BBMB.Budget.RequestId";
        private BudgetActorResolution? _resolved;

        public async Task<BudgetActor?> GetAsync(CancellationToken ct = default) => (await ResolveAsync(ct)).Actor;

        public async Task<BudgetActorResolution> ResolveAsync(CancellationToken ct = default)
        {
            if (_resolved is not null)
                return _resolved;
            _resolved = await ResolveCoreAsync(ct);
            if (_resolved.Actor is null)
                logger.LogWarning("Budget actor not resolved: {Reason}", _resolved.Reason);
            return _resolved;
        }

        private async Task<BudgetActorResolution> ResolveCoreAsync(CancellationToken ct)
        {
            var http = httpContextAccessor.HttpContext;
            if (http is null)
                return BudgetActorResolution.None("NO_HTTP_CONTEXT");

            var role = BudgetActingRole.Get(http);
            if (string.IsNullOrWhiteSpace(role))
                return BudgetActorResolution.None("NO_ACTING_ROLE");

            // Effective-admin acting roles resolve without an office-level-matched operational charge
            // (same AdminRoles signal BudgetRightHandler authorises on; read from the DB-backed config).
            var snapshot = await configProvider.GetAsync(ct);
            if (snapshot.AdminRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                return await ResolveAdminActorAsync(http, role, ct);

            return await ResolveOperationalActorAsync(http, role, ct);
        }

        /// <summary>Today's path, unchanged: a non-admin role must resolve through an office-level charge.</summary>
        private async Task<BudgetActorResolution> ResolveOperationalActorAsync(HttpContext http, string role, CancellationToken ct)
        {
            var charge = await chargeResolver.GetCurrentBudgetChargeAsync(ct);
            if (charge.Outcome == BudgetChargeOutcome.Ambiguous)
                return new BudgetActorResolution(null, charge.ChargeIds, "AMBIGUOUS_CHARGE");
            if (charge.Charge is null)
                return BudgetActorResolution.None("NO_CHARGE");

            var office = await officeScope.ResolveActorOfficeAsync(charge.Charge, role, ct);
            if (office is null)
                return BudgetActorResolution.None("NO_OFFICE");

            var name = await employeeContext.Employees.AsNoTracking()
                .Where(e => e.Id == charge.Charge.EmployeeId)
                .Select(e => e.NameEnglish)
                .FirstOrDefaultAsync(ct);
            if (string.IsNullOrWhiteSpace(name))
                return BudgetActorResolution.None("NO_EMPLOYEE_NAME");

            return new BudgetActorResolution(
                new BudgetActor(
                    charge.Charge.EmployeeId,
                    charge.Charge.ChargeId,
                    office.OfficeId,
                    office.OfficeLevel,
                    role,
                    name,
                    OnBehalfOfPost: null,
                    http.Connection.RemoteIpAddress ?? IPAddress.None,
                    RequestId(http)),
                [],
                null);
        }

        /// <summary>
        /// Effective-admin path: office from the admin role's level, charge only as the audit stamp the DB
        /// trigger requires. The record shape is unchanged; no schema change.
        /// </summary>
        private async Task<BudgetActorResolution> ResolveAdminActorAsync(HttpContext http, string role, CancellationToken ct)
        {
            if (!Guid.TryParse(JwtClaimHelper.GetUserId(http.User), out var userId))
                return BudgetActorResolution.None("NO_USER_ID");

            // All employee rows for the user (duplicate rows per user exist), as in ChargeResolver.
            var employeeIds = await employeeContext.Employees.AsNoTracking()
                .Where(e => e.UserId == userId)
                .Select(e => e.Id)
                .ToListAsync(ct);
            if (employeeIds.Count == 0)
                return BudgetActorResolution.None("NO_EMPLOYEE");

            // Admin office: the single active office of the admin role's configured level. FINANCE /
            // SECRETARIAT / IT_CELL are role-level, so charge ids are unused; an empty charge is sufficient.
            var office = await officeScope.ResolveActorOfficeAsync(new UserChargeContext { EmployeeId = employeeIds[0] }, role, ct);
            if (office is null)
                return BudgetActorResolution.None("NO_ADMIN_OFFICE");

            // Audit stamp charge: budget.fn_require_actor requires actor_charge_id to be a current active
            // charge of the actor employee (any project / level). Prefer a budget-project charge, else any
            // current charge. No usable charge at all ⇒ NO_ADMIN_CHARGE (attach a charge), never a sentinel.
            var snapshot = await configProvider.GetAsync(ct);
            var stamp = await ResolveAdminStampChargeAsync(employeeIds, snapshot.ProjectCode, ct);
            if (stamp is null)
                return BudgetActorResolution.None("NO_ADMIN_CHARGE");

            var name = await employeeContext.Employees.AsNoTracking()
                .Where(e => e.Id == stamp.EmployeeId)
                .Select(e => e.NameEnglish)
                .FirstOrDefaultAsync(ct);
            if (string.IsNullOrWhiteSpace(name))
                return BudgetActorResolution.None("NO_EMPLOYEE_NAME");

            return new BudgetActorResolution(
                new BudgetActor(
                    stamp.EmployeeId,
                    stamp.ChargeId,
                    office.OfficeId,
                    office.OfficeLevel,
                    role,
                    name,
                    OnBehalfOfPost: null,
                    http.Connection.RemoteIpAddress ?? IPAddress.None,
                    RequestId(http)),
                [],
                null);
        }

        /// <summary>
        /// A current active charge of the employee to stamp the audit actor with — exactly what
        /// <c>budget.fn_require_actor</c> accepts (active, not expired, not past its expiry). A budget-project
        /// charge is preferred so the stamp stays within the Budget project where one exists; otherwise any
        /// current charge, because the trigger does not restrict the stamp charge by project. Null if none.
        /// </summary>
        private async Task<EmployeeStampCharge?> ResolveAdminStampChargeAsync(
            IReadOnlyList<Guid> employeeIds, string projectCode, CancellationToken ct)
        {
            // A blank project code means no active settings row; the admin branch is unreachable then
            // (AdminRoles is empty on the empty snapshot), so fail closed rather than match a blank-project row.
            if (string.IsNullOrWhiteSpace(projectCode))
                return null;

            var now = DateTime.UtcNow;
            var current = employeeContext.EmployeeCharges.AsNoTracking()
                .Where(c =>
                    employeeIds.Contains(c.EmployeeId) &&
                    c.IsActive &&
                    !c.IsChargeExpire &&
                    (c.ChargeExpireTime == null || c.ChargeExpireTime > now));

            // Prefer a budget-project charge (deterministic order), else any current charge.
            var budget = await current
                .Where(c => c.ProjectCode == projectCode)
                .OrderBy(c => c.ChargeTime).ThenBy(c => c.Id)
                .Select(c => new EmployeeStampCharge(c.EmployeeId, c.Id))
                .FirstOrDefaultAsync(ct);
            if (budget is not null)
                return budget;

            return await current
                .OrderBy(c => c.ChargeTime).ThenBy(c => c.Id)
                .Select(c => new EmployeeStampCharge(c.EmployeeId, c.Id))
                .FirstOrDefaultAsync(ct);
        }

        private static Guid RequestId(HttpContext http)
        {
            if (http.Items[RequestIdItemKey] is Guid existing)
                return existing;
            var requestId = Guid.NewGuid();
            http.Items[RequestIdItemKey] = requestId;
            return requestId;
        }

        private sealed record EmployeeStampCharge(Guid EmployeeId, Guid ChargeId);
    }
}
