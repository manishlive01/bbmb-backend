using BBMB.BudgetService.Features.BudgetProcess.Attachments.Download.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Download
{
    public sealed record DownloadAttachmentQuery(long AttachmentId) : IRequest<Response<AttachmentFileDto>>;
}
