namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Download.Dtos
{
    /// <summary>A verified attachment ready to stream: the bytes whose digest was just checked.</summary>
    public sealed class AttachmentFileDto
    {
        public required Stream Content { get; init; }
        public required string ContentType { get; init; }
        public required string FileName { get; init; }
    }
}
