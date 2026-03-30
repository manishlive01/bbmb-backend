using BBMB.BudgetService.Features.EmployeeCharges.GetListByUser.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetListByUser
{
    public class GetEmployeeChargeByUserHandler
        : IRequestHandler<GetEmployeeChargeByUserQuery, List<GetEmployeeChargeByUserResponseDto>>
    {
        private readonly EmployeeContext _employeeContext;
        private readonly BudgetContext _budgetContext;

        public GetEmployeeChargeByUserHandler(
            EmployeeContext employeeContext,
            BudgetContext budgetContext)
        {
            _employeeContext = employeeContext;
            _budgetContext = budgetContext;
        }

        public async Task<List<GetEmployeeChargeByUserResponseDto>> Handle(
            GetEmployeeChargeByUserQuery request,
            CancellationToken cancellationToken)
        {
            var userId = JwtClaimHelper.GetUserId(request.User);

            if (string.IsNullOrEmpty(userId))
                return new List<GetEmployeeChargeByUserResponseDto>();

            var userGuid = Guid.Parse(userId);

            // 🔥 STEP 1: Update expired charges in ONE DB HIT
            await _employeeContext.EmployeeCharges
                .Where(ec =>
                    ec.Employee.UserId == userGuid &&
                    ec.ChargeExpireTime.HasValue &&
                    ec.ChargeExpireTime <= DateTime.UtcNow &&
                    !ec.IsChargeExpire)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ec => ec.IsChargeExpire, true),
                    cancellationToken);


            // 🔹 STEP 2: Fetch employee charges
            var employeeCharges = await (
                from ec in _employeeContext.EmployeeCharges.AsNoTracking()
                join e in _employeeContext.Employees
                    on ec.EmployeeId equals e.Id
                where e.UserId == userGuid
                select ec
            ).ToListAsync(cancellationToken);

            if (!employeeCharges.Any())
                return new List<GetEmployeeChargeByUserResponseDto>();


            // 🔹 STEP 3: Load lookup tables
            var projects = await _budgetContext.Projects
                .AsNoTracking()
                .ToDictionaryAsync(x => x.ProjCode, cancellationToken);

            var ces = await _budgetContext.CEManagements
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var crs = await _budgetContext.CRManagements
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var divs = await _budgetContext.Divisions
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, cancellationToken);


            // 🔹 STEP 4: Map response
            var result = employeeCharges.Select(ec => new GetEmployeeChargeByUserResponseDto
            {
                ProjectCode = ec.ProjectCode,
                ProjDesc = projects.ContainsKey(ec.ProjectCode)
                    ? projects[ec.ProjectCode].ProjDesc
                    : "",

                CEId = ec.CEId,

                CECode = ec.CEId.HasValue && ces.ContainsKey(ec.CEId.Value)
                            ? ces[ec.CEId.Value].CECode
                            : 0,

                CEName = ec.CEId.HasValue && ces.ContainsKey(ec.CEId.Value)
                            ? ces[ec.CEId.Value].Name
                            : "",

                CRId = ec.CRId,
                CRCode = ec.CRId.HasValue && crs.ContainsKey(ec.CRId.Value)
                         ? crs[ec.CRId.Value].CRCode
                         : 0,
                CRName = ec.CRId.HasValue && crs.ContainsKey(ec.CRId.Value)
                         ? crs[ec.CRId.Value].Name
                         : "",

                DivisionId = ec.DivisionId,
                DivisionCode = ec.DivisionId.HasValue && divs.ContainsKey(ec.DivisionId.Value)
                            ? divs[ec.DivisionId.Value].DivisionCode
                            : 0,
                DivName = ec.DivisionId.HasValue && divs.ContainsKey(ec.DivisionId.Value)
                            ? divs[ec.DivisionId.Value].DivisionDesc
                            : "",


                IsChargeExpire = ec.IsChargeExpire
            }).ToList();

            return result;
        }
    }
}