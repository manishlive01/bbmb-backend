using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Employees.Update
{
    public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeCommand, Response>
    {
        private readonly EmployeeContext _context;
        private readonly IFileStorageService _fileStorage;

        public UpdateEmployeeHandler(
            EmployeeContext context,
            IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<Response> Handle(UpdateEmployeeCommand request, CancellationToken ct)
        {
            var dto = request.dto;

            var employeeExists = await _context.Employees
                .AnyAsync(x => x.Id == dto.PersonalInfo.UserId, ct);

            if (!employeeExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Employee not found"
                };
            }

            // ✅ 1. UPDATE EMPLOYEE (INCLUDING EmployeeCode)
            await _context.Employees
                .Where(x => x.Id == dto.PersonalInfo.UserId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.EmployeeCode, dto.PersonalInfo.EmployeeCode) // ✅ FIXED
                    .SetProperty(x => x.TitleId, dto.PersonalInfo.TitleId)
                    .SetProperty(x => x.NameEnglish, dto.PersonalInfo.NameEnglish)
                    .SetProperty(x => x.NameHindi, dto.PersonalInfo.NameHindi)
                    .SetProperty(x => x.GenderId, dto.PersonalInfo.GenderId)
                    .SetProperty(x => x.DateOfBirth, DateTime.SpecifyKind(dto.PersonalInfo.DateOfBirth, DateTimeKind.Utc))
                    .SetProperty(x => x.PhoneNumber, dto.PersonalInfo.PhoneNumber)
                    .SetProperty(x => x.Email, dto.PersonalInfo.Email)
                    .SetProperty(x => x.Address, dto.PersonalInfo.Address),
                    ct);

            // ✅ 2. UPDATE EMPLOYMENT (REMOVE EmployeeCode ❌)
            await _context.EmploymentDetails
                .Where(x => x.EmployeeId == dto.PersonalInfo.UserId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.SourceOfRecruitmentId, dto.Employment.SourceOfRecruitmentId)
                    .SetProperty(x => x.DesignationId, dto.Employment.DesignationId)
                    .SetProperty(x => x.Position, dto.Employment.Position)
                    .SetProperty(x => x.AppointmentTypeId, dto.Employment.AppointmentTypeId)
                    .SetProperty(x => x.AppointmentDate, DateTime.SpecifyKind(dto.Employment.AppointmentDate, DateTimeKind.Utc))
                    .SetProperty(x => x.DateOfJoining, DateTime.SpecifyKind(dto.Employment.DateOfJoining, DateTimeKind.Utc))
                    .SetProperty(x => x.RetirementDate,
                        dto.Employment.RetirementDate == null
                            ? null
                            : DateTime.SpecifyKind(dto.Employment.RetirementDate.Value, DateTimeKind.Utc)),
                    ct);

            // ✅ 3. PHOTO UPDATE
            if (dto.PhotoDocs?.Photo != null)
            {
                var photoPath = await _fileStorage.SaveFileAsync(
                    dto.PhotoDocs.Photo,
                    FileType.Employee,
                    null,
                    ct);

                var exists = await _context.PhotoDocuments
                    .AnyAsync(x => x.EmployeeId == dto.PersonalInfo.UserId, ct);

                if (exists)
                {
                    await _context.PhotoDocuments
                        .Where(x => x.EmployeeId == dto.PersonalInfo.UserId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(x => x.PhotoUrl, photoPath), ct);
                }
                else
                {
                    await _context.PhotoDocuments.AddAsync(new PhotoDocument
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = dto.PersonalInfo.UserId,
                        PhotoUrl = photoPath
                    }, ct);
                }
            }

            // ✅ 4. FAMILY MEMBERS (DELETE + INSERT)
            await _context.FamilyMembers
                .Where(x => x.EmployeeId == dto.PersonalInfo.UserId)
                .ExecuteDeleteAsync(ct);

            var newFamilyMembers = dto.FamilyMembers
                .Select(f => new FamilyMember
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = dto.PersonalInfo.UserId,
                    Name = f.Name,
                    FamilyRelationId = f.FamilyRelationId,
                    PhoneNumber = f.PhoneNumber,
                    Address = f.Address,
                    IsEmployee = f.IsEmployee,
                    PhotoUrl = null
                })
                .ToList();

            // ⚠️ File upload (minimal loop required)
            foreach (var (member, dtoMember) in newFamilyMembers.Zip(dto.FamilyMembers))
            {
                if (dtoMember.Photo != null)
                {
                    member.PhotoUrl = await _fileStorage.SaveFileAsync(
                        dtoMember.Photo,
                        FileType.Family,
                        dto.PersonalInfo.UserId.ToString(),
                        ct);
                }
            }

            await _context.FamilyMembers.AddRangeAsync(newFamilyMembers, ct);

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Employee updated successfully"
            };
        }
    }
}