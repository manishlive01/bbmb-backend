using BBMB.BudgetService.Features.Employees.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.Settings;
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

        public GetEmployeeByUserIdHandler(
            EmployeeContext context,
            IOptions<FileSettings> options)
        {
            _context = context;
            _fileSettings = options.Value;
        }

        public async Task<GetEmployeeResponseDto?> Handle(
            GetEmployeeByUserIdQuery request,
            CancellationToken ct)
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .Include(x => x.Titles)
                .Include(x => x.Genders)
                .Include(x => x.PhotoDocument)
                .Include(x => x.FamilyMembers)
                    .ThenInclude(f => f.FamilyRelation) // ✅ IMPORTANT
                .Include(x => x.EmploymentDetail)
                    .ThenInclude(x => x.Designations)
                .Include(x => x.EmploymentDetail)
                    .ThenInclude(x => x.SourceOfRecruitments)
                .Include(x => x.EmploymentDetail)
                    .ThenInclude(x => x.AppointmentTypes)
                .FirstOrDefaultAsync(x => x.Id == request.employeeId, ct);

            if (employee == null)
                return null;

            var baseUrl = _fileSettings.FileServerUrl;

            var dto = new GetEmployeeResponseDto
            {
                PersonalInfo = new PersonalInfoResponseDto
                {
                    EmployeeCode = employee.EmployeeCode, // ✅ FIXED
                    UserId = employee.Id,

                    TitleId = employee.TitleId,
                    TitleName = employee.Titles?.TitleName ?? "",

                    NameEnglish = employee.NameEnglish,
                    NameHindi = employee.NameHindi,

                    GenderId = employee.GenderId,
                    GenderName = employee.Genders?.GenderName ?? "",

                    DateOfBirth = employee.DateOfBirth,
                    PhoneNumber = employee.PhoneNumber,
                    Email = employee.Email,
                    Address = employee.Address
                },

                Employment = new EmploymentInfoResponseDto
                {
                    SourceOfRecruitmentId = employee.EmploymentDetail.SourceOfRecruitmentId,
                    SourceOfRecruitmentName = employee.EmploymentDetail.SourceOfRecruitments?.SourceName ?? "",

                    DesignationId = employee.EmploymentDetail.DesignationId,
                    DesignationName = employee.EmploymentDetail.Designations?.DesignationName ?? "",

                    Position = employee.EmploymentDetail.Position,

                    AppointmentTypeId = employee.EmploymentDetail.AppointmentTypeId,
                    AppointmentTypeName = employee.EmploymentDetail.AppointmentTypes?.AppointmentTypeName ?? "",

                    AppointmentDate = employee.EmploymentDetail.AppointmentDate,
                    DateOfJoining = employee.EmploymentDetail.DateOfJoining,
                    RetirementDate = employee.EmploymentDetail.RetirementDate
                },

                PhotoDocs = new PhotoDocumentResponseDto
                {
                    PhotoUrl = employee.PhotoDocument != null
                        ? $"{baseUrl}{employee.PhotoDocument.PhotoUrl}"
                        : ""
                },

                // ✅ FIXED FAMILY MEMBERS
                FamilyMembers = employee.FamilyMembers.Select(f => new FamilyMemberResponseDto
                {
                    Name = f.Name,

                    FamilyRelationId = f.FamilyRelationId, // ✅ FIXED
                    FamilyRelationName = f.FamilyRelation != null
                        ? f.FamilyRelation.FamilyRelationName
                        : "",

                    PhoneNumber = f.PhoneNumber,
                    Address = f.Address,
                    IsEmployee = f.IsEmployee,

                    PhotoUrl = !string.IsNullOrEmpty(f.PhotoUrl)
                        ? $"{baseUrl}{f.PhotoUrl}"
                        : null
                }).ToList()
            };

            return dto;
        }
    }
}