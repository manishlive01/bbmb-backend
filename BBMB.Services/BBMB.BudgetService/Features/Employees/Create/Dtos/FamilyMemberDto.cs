namespace BBMB.BudgetService.Features.Employees.Create.Dtos
{
    public class FamilyMemberDto
    {
        public string Name { get; set; } = null!;
        public int FamilyRelationId { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;

        public bool IsEmployee { get; set; }

        public IFormFile? Photo { get; set; }
    }
}
