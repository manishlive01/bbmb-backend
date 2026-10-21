using BBMB.BudgetService.Features.BudgetProcess.Attachments.Download.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Attachments.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Services.Attachments;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Download
{
    /// <summary>
    /// Design §4.8, §8.2 download row, DDD T-22 note. Order: actor (M-20) → row exists and the caller may view an
    /// owning record (else NOT_FOUND, §4.9) → the stored file is read once into memory (≤ 10 485 760 bytes) and
    /// its SHA-256 compared with <c>sha256_digest</c>. A missing file or a mismatch is an integrity incident:
    /// logged at Error, answered INTEGRITY (OI-13), and the file is not served. The bytes served are the bytes
    /// verified, so the file cannot change between the check and the response.
    /// </summary>
    public sealed class DownloadAttachmentHandler(
        BudgetContext context, IBudgetActorSource actorSource, AttachmentAccessRules access,
        IFileStorageService storage, ILogger<DownloadAttachmentHandler> logger)
        : IRequestHandler<DownloadAttachmentQuery, Response<AttachmentFileDto>>
    {
        public async Task<Response<AttachmentFileDto>> Handle(DownloadAttachmentQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<AttachmentFileDto>(BudgetMessages.M20);
            var row = await context.BudgetAttachments.AsNoTracking()
                .FirstOrDefaultAsync(a => a.AttachmentId == request.AttachmentId, ct);
            if (row is null || !await access.CanViewAsync(row, actor, ct))
                return BudgetResponses.Fail<AttachmentFileDto>(BudgetMessages.NotFound);

            var stored = storage.OpenRead(FileType.BudgetAttachment, row.StorageKey.ToString());
            if (stored is null)
            {
                logger.LogError("INTEGRITY: attachment {AttachmentId} stored file {StorageKey} is missing", row.AttachmentId, row.StorageKey);
                return BudgetResponses.Fail<AttachmentFileDto>(BudgetMessages.Integrity);
            }
            var buffer = new MemoryStream();
            await using (stored)
            {
                var chunk = new byte[81920];
                int read;
                while ((read = await stored.ReadAsync(chunk, ct)) > 0)
                {
                    if (buffer.Length + read > AttachmentContent.MaxSizeBytes)
                    {
                        logger.LogError("INTEGRITY: attachment {AttachmentId} stored file exceeds the T-22 limit", row.AttachmentId);
                        return BudgetResponses.Fail<AttachmentFileDto>(BudgetMessages.Integrity);
                    }
                    buffer.Write(chunk, 0, read);
                }
            }
            buffer.Position = 0;
            var digest = await AttachmentContent.Sha256HexAsync(buffer, ct);
            if (!string.Equals(digest, row.Sha256Digest, StringComparison.Ordinal))
            {
                logger.LogError("INTEGRITY: attachment {AttachmentId} digest mismatch (recorded {Recorded}, stored {Stored}); requested by {EmployeeId}",
                    row.AttachmentId, row.Sha256Digest, digest, actor.EmployeeId);
                return BudgetResponses.Fail<AttachmentFileDto>(BudgetMessages.Integrity);
            }
            buffer.Position = 0;
            return new Response<AttachmentFileDto>
            {
                IsSucced = true,
                Message = "Attachment verified",
                Data = new AttachmentFileDto { Content = buffer, ContentType = row.ContentType, FileName = row.OriginalFileName },
            };
        }
    }
}
