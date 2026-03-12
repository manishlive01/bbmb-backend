using BBMB.BudgetService.Features.Employees.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.Settings;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Features.Employees.GetById
{
    public class GetEmployeeByUserIdHandler
         : IRequestHandler<GetEmployeeByUserIdQuery, GetEmployeeResponseDto?>
    {
        private readonly EmployeeContext _context;
        private readonly FileSettings _fileSettings;
        public GetEmployeeByUserIdHandler(EmployeeContext context, IOptions<FileSettings> options)
        {
            _context = context;
            _fileSettings = options.Value;
        }

        public async Task<GetEmployeeResponseDto?> Handle(
     GetEmployeeByUserIdQuery request,
     CancellationToken ct)
        {
            var employee = await _context.Employees
                .Include(x => x.EmploymentDetail)
                .Include(x => x.PhotoDocument)
                .Include(x => x.FamilyMembers)
                .FirstOrDefaultAsync(x => x.Id == request.employeeId, ct);

            if (employee == null)
                return null;

            var dto = employee.Adapt<GetEmployeeResponseDto>();

            var baseUrl = _fileSettings.FileServerUrl;

            if (dto.PhotoDocs != null)
                dto.PhotoDocs.PhotoUrl = $"{baseUrl}{dto.PhotoDocs.PhotoUrl}";

            foreach (var family in dto.FamilyMembers)
            {
                if (!string.IsNullOrEmpty(family.PhotoUrl))
                    family.PhotoUrl = $"{baseUrl}{family.PhotoUrl}";
            }

            return dto;
        }
    }
}
