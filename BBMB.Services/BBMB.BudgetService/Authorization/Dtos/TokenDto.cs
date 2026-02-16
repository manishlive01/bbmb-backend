namespace BBMB.BudgetService.Authorization.Dtos
{
    public class ProjectRoleTokenDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public List<RoleTokenDto> Roles { get; set; } = new();
    }

    public class RoleTokenDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
