namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetManualOfAccountsCodes.Dtos
{
    /// <summary>
    /// Design §8.8 (BUD-048, BUD-049): the Manual of Accounts Codes as at <see cref="ProducedAt"/>, in the layout
    /// of R-04 — the six codes and nomenclatures, the complete code, the ACC-009 flag and the not-for-use mark.
    /// </summary>
    public sealed record ManualOfAccountsResultDto(DateTimeOffset ProducedAt, IReadOnlyList<ManualCodeDto> Codes);

    public sealed record ManualCodeDto(
        long AccountCodeId,
        string CompleteCode,
        string HeadDesc,
        string MajorAccCode, string? MajorAccDesc,
        string SubMajorAccCode, string? SubMajorAccDesc,
        string MinorAccCode, string? MinorAccDesc,
        string SubMinorAccCode, string? SubMinorAccDesc,
        string DetailAccCode, string? DetailAccDesc,
        string ObjectAccCode, string? ObjectAccDesc,
        string PartyEmpRequired,
        string Origin,
        bool NotForUse,
        string? NotForUseRef);
}
