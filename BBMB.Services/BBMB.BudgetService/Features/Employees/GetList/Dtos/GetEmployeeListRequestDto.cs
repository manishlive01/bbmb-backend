namespace BBMB.BudgetService.Features.Employees.GetList.Dtos
{
    public class GetEmployeeListRequestDto
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }

        public bool SortDescending { get; set; }

        public string? NameEnglish { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public string? Designation { get; set; }

        public string? Gender { get; set; }
    }
}
