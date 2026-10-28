
using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Employees.Create;

public class CreateEmployeeHandler
    : IRequestHandler<CreateEmployeeCommand, Response>
{
    private readonly EmployeeContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<CreateEmployeeHandler> _logger;

    public CreateEmployeeHandler(
        EmployeeContext context,
        IFileStorageService fileStorage,
        ILogger<CreateEmployeeHandler> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<Response> Handle(
        CreateEmployeeCommand request,
        CancellationToken ct)
    {
        var dto = request.dto;

        if (dto?.PersonalInfo == null || dto.Employment == null)
        {
            return new Response
            {
                IsSucced = false,
                Message = "Personal information and employment details are required."
            };
        }

        var personal = dto.PersonalInfo;
        var employment = dto.Employment;

        if (string.IsNullOrWhiteSpace(personal.NameEnglish) ||
            string.IsNullOrWhiteSpace(personal.Email) ||
            string.IsNullOrWhiteSpace(personal.PhoneNumber) ||
            string.IsNullOrWhiteSpace(personal.Address))
        {
            return new Response
            {
                IsSucced = false,
                Message = "Name, email, phone number and address are required."
            };
        }

        if (dto.FamilyMembers == null)
        {
            return new Response
            {
                IsSucced = false,
                Message = "Family members must be a valid list."
            };
        }

        try
        {
            var email = personal.Email.Trim();

            var exists = await _context.Employees
                .AnyAsync(x => x.Email == email, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Employee already exists with this email."
                };
            }

            var employeeId = Guid.NewGuid();

            // Explicit mapping: do not depend on nested Mapster mapping.
            var employee = new Employee
            {
                Id = employeeId,
                UserId = personal.UserId,
                EmployeeCode = personal.EmployeeCode,
                TitleId = personal.TitleId,
                NameEnglish = personal.NameEnglish.Trim(),
                NameHindi = personal.NameHindi,
                GenderId = personal.GenderId,
                DateOfBirth = DateTime.SpecifyKind(
                    personal.DateOfBirth, DateTimeKind.Utc),
                PhoneNumber = personal.PhoneNumber.Trim(),
                Email = email,
                Address = personal.Address.Trim()
            };

            employee.EmploymentDetail = new EmploymentDetail
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                SourceOfRecruitmentId = employment.SourceOfRecruitmentId,
                DesignationId = employment.DesignationId,
                Position = employment.Position,
                AppointmentTypeId = employment.AppointmentTypeId,
                AppointmentDate = DateTime.SpecifyKind(
                    employment.AppointmentDate, DateTimeKind.Utc),
                DateOfJoining = DateTime.SpecifyKind(
                    employment.DateOfJoining, DateTimeKind.Utc),
                RetirementDate = employment.RetirementDate.HasValue
                    ? DateTime.SpecifyKind(
                        employment.RetirementDate.Value,
                        DateTimeKind.Utc)
                    : null
            };

            // Upload files before saving database records.
            // If the database save fails, uploaded files may need cleanup.
            string? employeePhotoPath = null;

            var photo = dto.PhotoDocs?.Photo;

            if (photo is { Length: > 0 })
            {
                employeePhotoPath = await _fileStorage.SaveFileAsync(
                    photo,
                    FileType.Employee,
                    null,
                    ct);
            }

            // Preserve the current schema's required photo-document relationship.
            employee.PhotoDocument = new PhotoDocument
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeId,
                PhotoUrl = employeePhotoPath ?? string.Empty
            };

            employee.FamilyMembers = new List<FamilyMember>();

            foreach (var family in dto.FamilyMembers)
            {
                string? familyPhotoPath = null;

                if (family.Photo is { Length: > 0 })
                {
                    familyPhotoPath = await _fileStorage.SaveFileAsync(
                        family.Photo,
                        FileType.Family,
                        employeeId.ToString(),
                        ct);
                }

                employee.FamilyMembers.Add(new FamilyMember
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    Name = family.Name,
                    FamilyRelationId = family.FamilyRelationId,
                    PhoneNumber = family.PhoneNumber,
                    Address = family.Address,
                    IsEmployee = family.IsEmployee,
                    PhotoUrl = familyPhotoPath
                });
            }

            await _context.Employees.AddAsync(employee, ct);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Employee created successfully."
            };
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Database insert failed while creating employee. EmployeeCode: {EmployeeCode}",
                personal.EmployeeCode);

            // Do not expose SQL, schema details or exception text to clients.
            return new Response
            {
                IsSucced = false,
                Message = "Employee could not be saved. Check required fields, master records and database constraints."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while creating employee. EmployeeCode: {EmployeeCode}",
                personal.EmployeeCode);

            return new Response
            {
                IsSucced = false,
                Message = "An unexpected error occurred while creating the employee."
            };
        }
    }
}
