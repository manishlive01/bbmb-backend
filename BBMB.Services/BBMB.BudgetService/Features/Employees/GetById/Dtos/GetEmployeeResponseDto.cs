namespace BBMB.BudgetService.Features.Employees.GetById.Dtos
{
    public class GetEmployeeResponseDto
    {
        public PersonalInfoResponseDto PersonalInfo { get; set; } = new();
        public EmploymentInfoResponseDto Employment { get; set; } = new();
        public PhotoDocumentResponseDto PhotoDocs { get; set; } = new();
        public List<FamilyMemberResponseDto> FamilyMembers { get; set; } = new();
    }
    public class PersonalInfoResponseDto
    {
        public int EmployeeCode { get; set; }
        public Guid UserId { get; set; }

        public int TitleId { get; set; }
        public string TitleName { get; set; } = null!;
        public string NameEnglish { get; set; } = null!;
        public string? NameHindi { get; set; }

        public int GenderId { get; set; }
        public string GenderName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string Address { get; set; } = null!;
    }
    public class EmploymentInfoResponseDto
    {
        public int SourceOfRecruitmentId { get; set; } 
        public string SourceOfRecruitmentName { get; set; } = null!;
        public int DesignationId { get; set; }
        public string DesignationName { get; set; } = null!;
        public string Position { get; set; } = null!;
        public int AppointmentTypeId { get; set; }
        public string AppointmentTypeName { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime? RetirementDate { get; set; }
    }
    public class PhotoDocumentResponseDto
    {
        public string PhotoUrl { get; set; } = null!;
    }
    public class FamilyMemberResponseDto
    {
        public string Name { get; set; } = null!;
        public int FamilyRelationId { get; set; }
        public string FamilyRelationName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;

        public bool IsEmployee { get; set; }

        public string? PhotoUrl { get; set; }
    }
} 
