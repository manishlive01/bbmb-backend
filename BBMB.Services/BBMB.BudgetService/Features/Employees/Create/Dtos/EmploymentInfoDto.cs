namespace BBMB.BudgetService.Features.Employees.Create.Dtos
{
    public class EmploymentInfoDto
    {
        public string SourceOfRecruitment { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string AppointmentType { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime? RetirementDate { get; set; }
    }
}
