using BBMB.BudgetService.Features.EmployeeCharges.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetList
{
    public class GetEmployeeChargeHandler
        : IRequestHandler<GetEmployeeChargeQuery, List<GetEmployeeChargeResponseDto>>
    {
        private readonly EmployeeContext _employeeContext;
        private readonly BudgetContext _budgetContext;
        private readonly FileSettings _fileSettings;

        public GetEmployeeChargeHandler(
            EmployeeContext employeeContext,
            BudgetContext budgetContext,
            IOptions<FileSettings> fileOptions)
        {
            _employeeContext = employeeContext;
            _budgetContext = budgetContext;
            _fileSettings = fileOptions.Value;
        }

        public async Task<List<GetEmployeeChargeResponseDto>> Handle(
            GetEmployeeChargeQuery request,
            CancellationToken ct)
        {
            var fileServerUrl = _fileSettings.FileServerUrl;

            // 🔹 STEP 1: Load lookup tables (Budget DB)
            var ces = await _budgetContext.CEManagements
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, ct);

            var crs = await _budgetContext.CRManagements
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, ct);

            var divs = await _budgetContext.Divisions
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, ct);


            // 🔹 STEP 2: Fetch employee charges (Employee DB)
            var employeeCharges = await _employeeContext.EmployeeCharges
                .AsNoTracking()
                .Include(x => x.Employee)
                    .ThenInclude(e => e.PhotoDocument)
                .ToListAsync(ct);


            // 🔹 STEP 3: Map response
            var result = employeeCharges.Select(ec => new GetEmployeeChargeResponseDto
            {
                Id = ec.Id,
                EmployeeId = ec.EmployeeId,

                EmployeeName = ec.Employee.NameEnglish,

                ProfileImageUrl = ec.Employee.PhotoDocument != null
                    ? fileServerUrl + ec.Employee.PhotoDocument.PhotoUrl
                    : null,

                ProjectCode = ec.ProjectCode,

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

                ChargeTime = ec.ChargeTime,
                ChargeExpireTime = ec.ChargeExpireTime,
                IsActive = ec.IsActive,
                IsChargeExpire = ec.IsChargeExpire
            }).ToList();

            return result;
        }
    }
}