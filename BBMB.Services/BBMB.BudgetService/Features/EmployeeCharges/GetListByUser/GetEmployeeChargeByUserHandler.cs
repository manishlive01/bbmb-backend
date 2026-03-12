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

            // 🔹 Step 1: Get employee charges
            var employeeCharges = await (
                from ec in _employeeContext.EmployeeCharges
                join e in _employeeContext.Employees
                    on ec.EmployeeId equals e.Id
                where e.UserId == userGuid
                select ec
            ).AsNoTracking().ToListAsync(cancellationToken);

            if (!employeeCharges.Any())
                return new List<GetEmployeeChargeByUserResponseDto>();


            // 🔹 Step 2: Get lookup tables from Budget DB
            var projects = await _budgetContext.Projects
                .AsNoTracking()
                .ToDictionaryAsync(x => x.ProjCode, cancellationToken);

            var ces = await _budgetContext.CEManagements
                .AsNoTracking()
                .ToDictionaryAsync(x => x.CECode, cancellationToken);

            var crs = await _budgetContext.CRManagements
                .AsNoTracking()
                .ToDictionaryAsync(x => x.CRCode, cancellationToken);

            var divs = await _budgetContext.Divisions
                .AsNoTracking()
                .ToDictionaryAsync(x => x.DivisionCode, cancellationToken);


            // 🔹 Step 3: Map response
            var result = employeeCharges.Select(ec => new GetEmployeeChargeByUserResponseDto
            {
                ProjectCode = ec.ProjectCode,
                ProjDesc = projects.ContainsKey(ec.ProjectCode)
                    ? projects[ec.ProjectCode].ProjDesc
                    : "",

                CECode = ec.CECode,
                CEName = ces.ContainsKey(ec.CECode)
                    ? ces[ec.CECode].Name
                    : "",

                CRCode = ec.CRCode,
                CRName = crs.ContainsKey(ec.CRCode)
                    ? crs[ec.CRCode].Name
                    : "",

                DivisionCode = ec.DivisionCode,
                DivName = divs.ContainsKey(ec.DivisionCode)
                    ? divs[ec.DivisionCode].DivisionDesc
                    : ""
            }).ToList();

            return result;
        }
    }
}