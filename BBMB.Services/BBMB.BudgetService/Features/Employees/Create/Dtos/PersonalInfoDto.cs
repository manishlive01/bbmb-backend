namespace BBMB.BudgetService.Features.Employees.Create.Dtos
{
    public class PersonalInfoDto
    {
        public Guid UserId {  get; set; }
        public string ApplicantTitle { get; set; } = null!;
        public string NameEnglish { get; set; } = null!;
        public string? NameHindi { get; set; }
        public string Gender { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string Address { get; set; } = null!;
    }
}
