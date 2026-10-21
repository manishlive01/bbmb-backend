namespace BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Search.Dtos
{
    /// <summary>Design §8.8 (F-27): a page of audit rows with the total matched for paging.</summary>
    public sealed record AuditSearchResultDto(int Page, int PageSize, long TotalCount, IReadOnlyList<AuditRowDto> Rows);

    /// <summary>One T-51 audit row: what changed, by whom, and the before/after values (BUD-215).</summary>
    public sealed record AuditRowDto(
        long AuditId,
        DateTimeOffset OccurredAt,
        string TableName,
        string RecordKey,
        string Operation,
        string? ColumnName,
        string? OldValue,
        string? NewValue,
        string? ActionCode,
        Guid ActorEmployeeId,
        string ActorName,
        long ActorOfficeId,
        string ActorRoleCode);
}
