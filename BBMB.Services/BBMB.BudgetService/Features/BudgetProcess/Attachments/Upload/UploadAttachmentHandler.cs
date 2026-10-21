using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Services.Attachments;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload
{
    /// <summary>
    /// Design §4.8 (BUD-018, BUD-019, DDD T-22). Order: actor (M-20) → extension, evidence class and declared size
    /// (M-24) → the file is buffered once, at most 10 485 760 bytes (M-24 beyond) → content type from the bytes
    /// (M-24 on mismatch) → SHA-256 → scan → store under a new storage key → T-22 row in the action scope.
    /// Detection, digest, scan and storage all read the same buffer from position 0, so the bytes scanned, the
    /// bytes stored and <c>sha256_digest</c> are the same (review of P2-03). A failed scan stores no file and no
    /// row. If the row is not written the stored file is deleted best-effort and the failure is rethrown.
    /// The right (Attachment.Upload) is on the endpoint; ownership is checked where the attachment is referenced.
    /// </summary>
    public sealed class UploadAttachmentHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope,
        IAttachmentScanner scanner, IFileStorageService storage, ILogger<UploadAttachmentHandler> logger,
        TimeProvider? clock = null)
        : IRequestHandler<UploadAttachmentCommand, UploadAttachmentResponse>
    {
        public const string ActionCode = "ATTACHMENT_UPLOAD";
        private const int HeadLength = 16;

        public async Task<UploadAttachmentResponse> Handle(UploadAttachmentCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return Fail(BudgetMessages.M20);
            var file = request.File;
            var extension = AttachmentContent.ExtensionOf(file.FileName);
            if (!AttachmentContent.AllowedExtensions.Contains(extension)
                || (request.EvidenceClass == AttachmentEvidenceClass.Evidence && !AttachmentContent.EvidenceExtensions.Contains(extension))
                || file.Length <= 0 || file.Length > AttachmentContent.MaxSizeBytes
                || file.FileName.Length > 255)
                return Fail(BudgetMessages.M24);

            await using var buffer = await BufferAsync(file, ct);
            if (buffer is null)
                return Fail(BudgetMessages.M24);

            var head = buffer.GetBuffer().AsSpan(0, (int)Math.Min(HeadLength, buffer.Length));
            var contentType = AttachmentContent.DetectContentType(extension, head);
            if (contentType is null)
                return Fail(BudgetMessages.M24);

            buffer.Position = 0;
            var digest = await AttachmentContent.Sha256HexAsync(buffer, ct);

            buffer.Position = 0;
            var scan = await scanner.ScanAsync(buffer, ct);
            switch (scan.Verdict)
            {
                case ScanVerdict.Unavailable:
                    logger.LogError("Attachment scanner unavailable ({Engine}): {Detail}", scan.Engine, scan.Detail);
                    return new UploadAttachmentResponse
                    {
                        IsSucced = false, MessageId = BudgetMessages.M24, ScannerUnavailable = true,
                        Message = BudgetMessages.Format(BudgetMessages.M24) + " Malware scanning is unavailable; the file was not accepted.",
                    };
                case ScanVerdict.Infected:
                    logger.LogWarning("Attachment refused by malware scan: digest {Digest}, uploader {EmployeeId}, signature {Detail}",
                        digest, actor.EmployeeId, scan.Detail);
                    return new UploadAttachmentResponse
                    {
                        IsSucced = false, MessageId = BudgetMessages.M24,
                        Message = BudgetMessages.Format(BudgetMessages.M24) + " The file failed the malware scan.",
                    };
            }

            var row = new BudgetAttachment
            {
                StorageKey = Guid.NewGuid(),
                OriginalFileName = Path.GetFileName(file.FileName),
                FileExtension = extension,
                ContentType = contentType,
                EvidenceClass = request.EvidenceClass,
                SizeBytes = buffer.Length,
                Sha256Digest = digest,
                ScanEngine = scan.Engine.Length > 100 ? scan.Engine[..100] : scan.Engine,
                ScannedAt = (clock ?? TimeProvider.System).GetUtcNow(),
            };
            var storageName = row.StorageKey.ToString();
            buffer.Position = 0;
            await storage.SaveFileAsync(buffer, FileType.BudgetAttachment, storageName, ct);
            try
            {
                await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
                context.BudgetAttachments.Add(row);
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                storage.TryDelete(FileType.BudgetAttachment, storageName);
                if (storage.OpenRead(FileType.BudgetAttachment, storageName) is { } orphan)
                {
                    await orphan.DisposeAsync();
                    logger.LogWarning("Attachment row not written and the stored file could not be deleted: orphan {StorageKey}", storageName);
                }
                throw;
            }
            return new UploadAttachmentResponse
            {
                IsSucced = true,
                Message = "Attachment uploaded",
                Data = new UploadAttachmentResponseDto { AttachmentId = row.AttachmentId, Sha256 = digest, SizeBytes = row.SizeBytes },
            };
        }

        /// <summary>
        /// Copies the upload into memory, refusing (null) a stream longer than the T-22 limit whatever the declared
        /// length was. The buffer is the single source of every later step.
        /// </summary>
        private static async Task<MemoryStream?> BufferAsync(IFormFile file, CancellationToken ct)
        {
            var buffer = new MemoryStream((int)Math.Min(file.Length, AttachmentContent.MaxSizeBytes));
            await using var source = file.OpenReadStream();
            var chunk = new byte[81920];
            int read;
            while ((read = await source.ReadAsync(chunk, ct)) > 0)
            {
                if (buffer.Length + read > AttachmentContent.MaxSizeBytes)
                {
                    await buffer.DisposeAsync();
                    return null;
                }
                buffer.Write(chunk, 0, read);
            }
            if (buffer.Length == 0)
            {
                await buffer.DisposeAsync();
                return null;
            }
            return buffer;
        }

        private static UploadAttachmentResponse Fail(string messageId) => new()
        {
            IsSucced = false, MessageId = messageId, Message = BudgetMessages.Format(messageId),
        };
    }
}
