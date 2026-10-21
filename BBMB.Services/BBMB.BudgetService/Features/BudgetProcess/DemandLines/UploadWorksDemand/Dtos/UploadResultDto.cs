namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.UploadWorksDemand.Dtos
{
    /// <summary>The outcome of an accepted upload (design §8.5): the number of lines committed (BUD-101).</summary>
    public sealed record UploadResultDto(long UploadBatchId, int LinesCommitted);
}
