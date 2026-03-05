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
        public Guid UserId { get; set; }

        public string ApplicantTitle { get; set; } = null!;
        public string NameEnglish { get; set; } = null!;
        public string? NameHindi { get; set; }

        public string Gender { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string Address { get; set; } = null!;
    }
    public class EmploymentInfoResponseDto
    {
        public string SourceOfRecruitment { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string AppointmentType { get; set; } = null!;

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
        public string Relation { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;

        public bool IsEmployee { get; set; }

        public string? PhotoUrl { get; set; }
    }
} 
