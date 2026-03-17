using BBMB.BudgetService.Features.EmployeeCharges.GetByUser.Dtos;
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

            var result =
                from ec in _employeeContext.EmployeeCharges.AsNoTracking()

                join ce in _budgetContext.CEManagements.AsNoTracking()
                    on ec.CECode equals ce.CECode into ceJoin
                from ce in ceJoin.DefaultIfEmpty()

                join cr in _budgetContext.CRManagements.AsNoTracking()
                    on ec.CRCode equals cr.CRCode into crJoin
                from cr in crJoin.DefaultIfEmpty()

                join div in _budgetContext.Divisions.AsNoTracking()
                    on ec.DivisionCode equals div.DivisionCode into divJoin
                from div in divJoin.DefaultIfEmpty()

                select new GetEmployeeChargeResponseDto
                {
                    Id = ec.Id,
                    EmployeeId = ec.EmployeeId,

                    EmployeeName = ec.Employee.NameEnglish,

                    ProfileImageUrl = ec.Employee.PhotoDocument != null
                        ? fileServerUrl + ec.Employee.PhotoDocument.PhotoUrl
                        : null,

                    ProjectCode = ec.ProjectCode,

                    CECode = ec.CECode,
                    CEName = ce != null ? ce.Name : "",

                    CRCode = ec.CRCode,
                    CRName = cr != null ? cr.Name : "",

                    DivisionCode = ec.DivisionCode,
                    DivName = div != null ? div.DivisionDesc : "",

                    ChargeTime = ec.ChargeTime,
                    ChargeExpireTime = ec.ChargeExpireTime,
                    IsActive = ec.IsActive,
                    IsChargeExpire = ec.IsChargeExpire
                };

            return await result.ToListAsync(ct);
        }
    }
}