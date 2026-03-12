namespace BBMB.BudgetService.Features.Employees.Create.Dtos
{
    public class PersonalInfoDto
    {
        public Guid UserId {  get; set; }
        public int TitleId { get; set; }
        public string NameEnglish { get; set; } = null!;
        public string? NameHindi { get; set; }
        public int GenderId { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;

        public string Address { get; set; } = null!;
    }
}
