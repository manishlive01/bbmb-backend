namespace BBMB.BudgetService.Features.Employees.Create.Dtos
{
    public class EmploymentInfoDto
    {
        public int SourceOfRecruitmentId { get; set; }
        public int DesignationId { get; set; } 
        public string Position { get; set; } = null!;
        public int AppointmentTypeId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public DateTime DateOfJoining { get; set; }
        public DateTime? RetirementDate { get; set; }
    }
}
