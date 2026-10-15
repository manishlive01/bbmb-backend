namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetRegister.Dtos
{
    /// <summary>Design §8.8: one approval with all its versions (F-05, BUD-066).</summary>
    public sealed record ApprovalRegisterRowDto(
        long AdminApprovalId,
        long WorkId,
        long DivisionOfficeId,
        int SerialNo,
        short CurrentVersionNo,
        IReadOnlyList<ApprovalVersionDto> Versions);

    public sealed record ApprovalVersionDto(
        short VersionNo,
        string NameOfWork,
        string AaReferenceNo,
        DateOnly DateOfApproval,
        long AccountCodeId,
        decimal AmountSanctioned,
        string CompetentAuthority);
}
