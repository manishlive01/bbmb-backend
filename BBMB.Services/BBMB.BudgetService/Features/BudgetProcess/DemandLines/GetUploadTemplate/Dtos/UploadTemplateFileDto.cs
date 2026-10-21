namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetUploadTemplate.Dtos
{
    /// <summary>The template workbook returned to the officer (BUD-097).</summary>
    public sealed record UploadTemplateFileDto(byte[] Content, string ContentType, string FileName);
}
