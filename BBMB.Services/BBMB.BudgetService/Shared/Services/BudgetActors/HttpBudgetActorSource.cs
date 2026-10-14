using System.Net;
using Auth.Authorization.Handlers;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Shared.Services.EmployeeChargeServices;
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
    public sealed class HttpBudgetActorSource(
        IHttpContextAccessor httpContextAccessor,
        IChargeResolver chargeResolver,
        IBudgetOfficeScope officeScope,
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

            if (http.Items[RequestIdItemKey] is not Guid requestId)
            {
                requestId = Guid.NewGuid();
                http.Items[RequestIdItemKey] = requestId;
            }

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
                    requestId),
                [],
                null);
        }
    }
}
