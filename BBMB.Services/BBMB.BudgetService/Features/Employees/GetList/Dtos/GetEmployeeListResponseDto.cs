namespace BBMB.BudgetService.Features.Employees.GetList.Dtos
{
    public class GetEmployeeListResponseDto
    {
        public Guid EmployeeId { get; set; }

        public string NameEnglish { get; set; } = null!;

        public int GenderId { get; set; }
        public String GenderName { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public int DesignationId { get; set; }
        public string DesignationName { get; set; }

        public DateTime DateOfJoining { get; set; }

        public string PhotoUrl { get; set; } = null!;
    }
}
