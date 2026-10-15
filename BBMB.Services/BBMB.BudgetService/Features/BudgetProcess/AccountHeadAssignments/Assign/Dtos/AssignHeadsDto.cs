namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign.Dtos
{
    /// <summary>Body of <c>POST /api/account-head-assignments</c> (F-17).</summary>
    public sealed class AssignHeadsDto
    {
        public long OfficeId { get; set; }
        public List<long> AccountCodeIds { get; set; } = [];
    }
}
