namespace BBMB.BudgetService.Features.Employees.GetList.Dtos
{
    public class GetEmployeeListResponseDto
    {
        public Guid EmployeeId { get; set; }

        public string NameEnglish { get; set; } = null!;

        public string Gender { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Designation { get; set; } = null!;

        public DateTime DateOfJoining { get; set; }

        public string PhotoUrl { get; set; } = null!;
    }
}
