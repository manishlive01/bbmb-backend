namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create.Dtos
{
    /// <summary>
    /// F-03 result. On an M-27 refusal only <see cref="ExceedsDelegation"/> is set (true): the client asks for a
    /// justification and resubmits (BUD-055).
    /// </summary>
    public sealed class CreateAdminApprovalResponseDto
    {
        public long AdminApprovalId { get; set; }
        public int SerialNo { get; set; }
        public int WorkId { get; set; }
        public bool ExceedsDelegation { get; set; }
        /// <summary>The BUD-059 line added to the current WORKS demand, or null when none was added.</summary>
        public long? DemandLineId { get; set; }
    }
}
