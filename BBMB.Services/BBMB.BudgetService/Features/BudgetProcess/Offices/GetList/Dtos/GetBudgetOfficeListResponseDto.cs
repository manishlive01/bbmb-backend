namespace BBMB.BudgetService.Features.BudgetProcess.Offices.GetList.Dtos
{
    /// <summary>One T-13 office; <see cref="OfficeName"/> is read from the master for a mapped office.</summary>
    public sealed class GetBudgetOfficeListResponseDto
    {
        public long OfficeId { get; set; }
        public string OfficeLevel { get; set; } = string.Empty;
        public string? OfficeName { get; set; }
        public int? DivisionId { get; set; }
        public int? CircleId { get; set; }
        public int? CeId { get; set; }
        public string? WingCode { get; set; }
        public long? ParentOfficeId { get; set; }
        public bool IsActive { get; set; }
    }
}
