using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.PublicMasters.Entities;
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

        [Column("employee_code")]
        [MaxLength(6)]
        public int EmployeeCode { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("title_id")]
        public int TitleId { get; set; } 

        [Column("name_english")]
        public string NameEnglish { get; set; } = null!;

        [Column("name_hindi")]
        public string? NameHindi { get; set; }

        [Column("gender_id")]
        public int GenderId { get; set; }

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


        [ForeignKey("GenderId")]
        public Gender Genders { get; set; } = null!;

        [ForeignKey("TitleId")]
        public Title Titles { get; set; } = null!;


    }


    [Table("employment_detail")]
    public class EmploymentDetail
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Column("source_of_recruitment_id")]
        public int SourceOfRecruitmentId { get; set; } 

        [Column("designation_id")]
        public int DesignationId { get; set; }

        [Column("position")]
        public string Position { get; set; } = null!;

        [Column("appointment_type_id")]
        public int AppointmentTypeId { get; set; }

        [Column("appointment_date")]
        public DateTime AppointmentDate { get; set; }

        [Column("date_of_joining")]
        public DateTime DateOfJoining { get; set; }

        [Column("retirement_date")]
        public DateTime? RetirementDate { get; set; }

        public Employee Employee { get; set; } = null!;

        [ForeignKey("DesignationId")]
        public Designation Designations { get; set; } = null!;

        [ForeignKey("SourceOfRecruitmentId")]
        public SourceOfRecruitment SourceOfRecruitments { get; set; } = null!;

        [ForeignKey("AppointmentTypeId")]
        public AppointmentType AppointmentTypes { get; set; } = null!;
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

        // ✅ FK instead of string
        [Column("family_relation_id")]
        public int FamilyRelationId { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; } = null!;

        [Column("address")]
        public string Address { get; set; } = null!;

        [Column("is_employee")]
        public bool IsEmployee { get; set; }

        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        public Employee Employee { get; set; } = null!;
        [ForeignKey(nameof(FamilyRelationId))]
        public FamilyRelation FamilyRelation { get; set; } = null!;

    }
}
