namespace BBMB.BudgetService.Features.Employees.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ApplicantTitle { get; set; } = null!;
        public string NameEnglish { get; set; } = null!;
        public string? NameHindi { get; set; }

        public string Gender { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string Address { get; set; } = null!;

        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();

        public EmploymentDetail EmploymentDetail { get; set; } = null!;
        public PhotoDocument PhotoDocument { get; set; } = null!;
    }
    public class EmploymentDetail
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string SourceOfRecruitment { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string AppointmentType { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime? RetirementDate { get; set; }

        public Employee Employee { get; set; } = null!;
    }
    public class PhotoDocument
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string PhotoUrl { get; set; } = null!;

        public Employee Employee { get; set; } = null!;
    }
    public class FamilyMember
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string Name { get; set; } = null!;
        public string Relation { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;

        public bool IsEmployee { get; set; }

        public string? PhotoUrl { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}
