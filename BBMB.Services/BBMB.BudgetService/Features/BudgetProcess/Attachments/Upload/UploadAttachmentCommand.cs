using BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload.Dtos;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload
{
    /// <param name="File">The multipart <c>file</c> field.</param>
    /// <param name="EvidenceClass">EVIDENCE or SUPPORTING (T-22 <c>evidence_class</c>).</param>
    public sealed record UploadAttachmentCommand(IFormFile File, string EvidenceClass) : IRequest<UploadAttachmentResponse>;
}
