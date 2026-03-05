namespace BBMB.BudgetService.Features.Employees.Create.Dtos
{
    public class CreateEmployeeDto
    {
        public PersonalInfoDto PersonalInfo { get; set; } = new();
        public EmploymentInfoDto Employment { get; set; } = new();
        public PhotoDocumentDto PhotoDocs { get; set; } = new();
        public List<FamilyMemberDto> FamilyMembers { get; set; } = new();
    }
}
