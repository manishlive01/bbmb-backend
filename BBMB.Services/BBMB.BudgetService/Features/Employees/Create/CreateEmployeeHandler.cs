using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Settings;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Features.Employees.Create
{

    public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, Response>
    {
        private readonly EmployeeContext _context;
        private readonly IFileStorageService _fileStorage;
        public CreateEmployeeHandler(
    EmployeeContext context,
    IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Response> Handle(CreateEmployeeCommand request, CancellationToken ct)
        {
            var dto = request.dto;

            // Check duplicate email
            var exists = await _context.Employees
                .AnyAsync(x => x.Email == dto.PersonalInfo.Email, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Employee already exists with this email"
                };
            }

            var employee = dto.Adapt<Employee>();
            employee.Id = Guid.NewGuid();

            // Employment
            employee.EmploymentDetail = new EmploymentDetail
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                SourceOfRecruitment = dto.Employment.SourceOfRecruitment,
                Designation = dto.Employment.Designation,
                Position = dto.Employment.Position,
                AppointmentType = dto.Employment.AppointmentType,
                AppointmentDate = dto.Employment.AppointmentDate,
                DateOfJoining = dto.Employment.DateOfJoining,
                RetirementDate = dto.Employment.RetirementDate
            };

            // Photo
            string employeePhotoPath = "";

            if (dto.PhotoDocs.Photo != null)
            {
                employeePhotoPath = await _fileStorage.SaveFileAsync(
         dto.PhotoDocs.Photo,
         FileType.Employee,
         null,
         ct);
            }

            employee.PhotoDocument = new PhotoDocument
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                PhotoUrl = employeePhotoPath
            };
            // Family Members
            employee.FamilyMembers = new List<FamilyMember>();

            foreach (var f in dto.FamilyMembers)
            {
                string? photoPath = null;

                if (f.Photo != null)
                    photoPath = await _fileStorage.SaveFileAsync(
           f.Photo,
           FileType.Family,
           employee.Id.ToString(),
           ct);

                employee.FamilyMembers.Add(new FamilyMember
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employee.Id,
                    Name = f.Name,
                    Relation = f.Relation,
                    PhoneNumber = f.PhoneNumber,
                    Address = f.Address,
                    IsEmployee = f.IsEmployee,
                    PhotoUrl = photoPath
                });
            }

            await     _context.Employees.AddAsync(employee);

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Employee created successfully"
            };
        }
       
    }
}
