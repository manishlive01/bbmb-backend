using BBMB.BudgetService.Features.EmployeeCharges.GetByUser.Dtos;
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
        private readonly EmployeeContext _context;
        private readonly FileSettings _fileSettings;

        public GetEmployeeChargeHandler(
            EmployeeContext context,
            IOptions<FileSettings> fileOptions)
        {
            _context = context;
            _fileSettings = fileOptions.Value;
        }

        public async Task<List<GetEmployeeChargeResponseDto>> Handle(
            GetEmployeeChargeQuery request,
            CancellationToken ct)
        {
            var fileServerUrl = _fileSettings.FileServerUrl;

            return await _context.EmployeeCharges
                .AsNoTracking()
                .Select(ec => new GetEmployeeChargeResponseDto
                {
                    Id = ec.Id,
                    EmployeeId = ec.EmployeeId,

                    EmployeeName = ec.Employee.NameEnglish,

                    ProfileImageUrl = ec.Employee.PhotoDocument != null
                        ? fileServerUrl + ec.Employee.PhotoDocument.PhotoUrl
                        : null,

                    ProjectCode = ec.ProjectCode,
                    CECode = ec.CECode,
                    CRCode = ec.CRCode,
                    DivisionCode = ec.DivisionCode,
                    ChargeTime = ec.ChargeTime,
                    ChargeExpireTime = ec.ChargeExpireTime,
                    IsActive = ec.IsActive,
                    IsChargeExpire = ec.IsChargeExpire
                })
                .ToListAsync(ct);
        }
    }
}