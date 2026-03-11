using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Employees.Entities
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("applicant_title")]
        public string ApplicantTitle { get; set; } = null!;

        [Column("name_english")]
        public string NameEnglish { get; set; } = null!;

        [Column("name_hindi")]
        public string? NameHindi { get; set; }

        [Column("gender")]
        public string Gender { get; set; } = null!;

        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("address")]
        public string Address { get; set; } = null!;

        public ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();

        public EmploymentDetail EmploymentDetail { get; set; } = null!;

        public PhotoDocument PhotoDocument { get; set; } = null!;

        public ICollection<EmployeeCharge> Charges { get; set; } = new List<EmployeeCharge>();
    }


    [Table("employment_detail")]
    public class EmploymentDetail
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Column("source_of_recruitment")]
        public string SourceOfRecruitment { get; set; } = null!;

        [Column("designation")]
        public string Designation { get; set; } = null!;

        [Column("position")]
        public string Position { get; set; } = null!;

        [Column("appointment_type")]
        public string AppointmentType { get; set; } = null!;

        [Column("appointment_date")]
        public DateTime AppointmentDate { get; set; }

        [Column("date_of_joining")]
        public DateTime DateOfJoining { get; set; }

        [Column("retirement_date")]
        public DateTime? RetirementDate { get; set; }

        public Employee Employee { get; set; } = null!;
    }

    [Table("employee_photo_document")]
    public class PhotoDocument
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Column("photo_url")]
        public string PhotoUrl { get; set; } = null!;

        public Employee Employee { get; set; } = null!;
    }


    [Table("family_member")]
    public class FamilyMember
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("relation")]
        public string Relation { get; set; } = null!;

        [Column("phone_number")]
        public string PhoneNumber { get; set; } = null!;

        [Column("address")]
        public string Address { get; set; } = null!;

        [Column("is_employee")]
        public bool IsEmployee { get; set; }

        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        public Employee Employee { get; set; } = null!;
    }
}
