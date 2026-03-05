using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Employees.Update
{
    public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeCommand, Response>
    {
        private readonly EmployeeContext _context;

        public UpdateEmployeeHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateEmployeeCommand request, CancellationToken ct)
        {
            var dto = request.dto;

            var employee = await _context.Employees
                .Include(x => x.EmploymentDetail)
                .Include(x => x.FamilyMembers)
                .Include(x => x.PhotoDocument)
                .FirstOrDefaultAsync(x => x.UserId == dto.PersonalInfo.UserId, ct);

            if (employee == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Employee not found"
                };
            }

            // Map personal info
            dto.Adapt(employee);

            // Update employment
            employee.EmploymentDetail.SourceOfRecruitment = dto.Employment.SourceOfRecruitment;
            employee.EmploymentDetail.Designation = dto.Employment.Designation;
            employee.EmploymentDetail.Position = dto.Employment.Position;
            employee.EmploymentDetail.AppointmentType = dto.Employment.AppointmentType;
            employee.EmploymentDetail.AppointmentDate = dto.Employment.AppointmentDate;
            employee.EmploymentDetail.DateOfJoining = dto.Employment.DateOfJoining;
            employee.EmploymentDetail.RetirementDate = dto.Employment.RetirementDate;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Employee updated successfully"
            };
        }
    }
}
