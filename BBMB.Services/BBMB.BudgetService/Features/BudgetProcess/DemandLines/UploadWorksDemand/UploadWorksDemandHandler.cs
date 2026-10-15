using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.UploadWorksDemand.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Settings;
using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services;
using BBMB.BudgetService.Shared.Services.Attachments;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.UploadWorksDemand
{
    /// <summary>
    /// Design §8.5 (F-08, BUD-098 to BUD-104). The works demand of a WORKS proposal in S-01, framed from a
    /// spreadsheet in the template of <see cref="GetUploadTemplate"/>.
    /// <list type="number">
    /// <item>The §4.9 origin-office write checks (<see cref="DemandLineWrites.ResolveAsync"/>): no actor → M-20;
    /// not visible → NOT_FOUND; F07.Enter → M-20; not the origin → M-20; not S-01 held by the origin → M-19;
    /// cycle not OPEN → M-17; another officer's lease → M-23. The category is not WORKS → M-19 (upload is a
    /// works function; the three head-wise categories upload under P3).</item>
    /// <item>The file is buffered once and opened as a workbook; a file that is not a readable workbook, or whose
    /// heading row is not <see cref="UploadTemplateColumns.Headings"/> exactly and in order, is refused with M-11
    /// and <b>not read further</b> (BUD-098).</item>
    /// <item>The data-row count must be ≤ <c>UPLOAD_MAX_LINES</c> (T-17, BUD-103); above it the file is refused
    /// with the count and the limit, nothing parsed.</item>
    /// <item>Every row is parsed into a candidate WORK line and evaluated together at moment <c>Upload</c>
    /// (<see cref="IDemandLineRules"/>): a bad row 4,999 of 5,000 fails the whole upload and is listed by its
    /// sheet row (BUD-100); a duplicate work is M-25 (VR-14, BUD-104). Each candidate carries its sheet row in
    /// <c>SerialNo</c> so a failure names the row; the committed lines are renumbered max + 1.</item>
    /// <item>On success, in one <c>UPLOAD</c> scope: the file as a T-22 SUPPORTING attachment, one T-34
    /// upload_batch, and the lines carrying its <c>upload_batch_id</c>; the T-31 figure versions the lines write
    /// carry <c>change_source = UPLOAD</c> (the trigger reads the action code). The lease is renewed (§4.10).</item>
    /// </list>
    /// </summary>
    public sealed class UploadWorksDemandHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, IBudgetOfficeScope officeScope,
        IAuthorizationService authorization, IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
        IDemandLineRules rules, WorkRules workRules, IBudgetSettingsReader settings,
        IAttachmentScanner scanner, IFileStorageService storage, ILogger<UploadWorksDemandHandler> logger,
        TimeProvider? clock = null)
        : IRequestHandler<UploadWorksDemandCommand, Response>
    {
        public const string ActionCode = "UPLOAD";
        private const int HeadLength = 16;

        public async Task<Response> Handle(UploadWorksDemandCommand request, CancellationToken ct)
        {
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var target = await DemandLineWrites.ResolveAsync(
                context, actorSource, officeScope, authorization, httpContextAccessor, request.ProposalId, now, ct);
            if (target.Refusal is { } refusal)
                return refusal;
            var (actor, p) = (target.Actor!, target.Proposal!);
            if (p.Category != ProposalCategory.Works)
                return BudgetResponses.Fail(BudgetMessages.M19);

            // Buffer the file once: it is both the data to read and the T-22 file to store (one read, one copy).
            await using var buffer = await BufferAsync(request.File, ct);
            if (buffer is null)
                return BudgetResponses.Fail(BudgetMessages.M11);

            ExcelPackage.License.SetNonCommercialPersonal("BBMB");
            ExcelWorksheet? sheet;
            try
            {
                buffer.Position = 0;
                var package = new ExcelPackage(buffer);
                sheet = package.Workbook.Worksheets.FirstOrDefault();
            }
            catch
            {
                // Not a readable workbook: M-11, never the parser's exception text (BUD-098, §4.9).
                return BudgetResponses.Fail(BudgetMessages.M11);
            }
            if (sheet is null || !HeadingsMatch(sheet))
                return BudgetResponses.Fail(BudgetMessages.M11);

            var rowCount = sheet.Dimension?.Rows ?? 0;
            var dataRows = Math.Max(0, rowCount - 1);
            var maxLines = (int)((await settings.GetAsync(SettingKey.UploadMaxLines, now, ct))?.NumericValue ?? 0);
            if (maxLines > 0 && dataRows > maxLines)
                return new Response
                {
                    IsSucced = false,
                    MessageId = BudgetMessages.M11,
                    Message = $"The file carries {dataRows} lines; the limit is {maxLines}. Reduce the file and upload again.",
                };
            if (dataRows == 0)
                return BudgetResponses.Fail(BudgetMessages.M11);

            // Parse each row into a candidate WORK line; the sheet row (2..n) is kept in SerialNo so a refusal
            // names the row and the duplicate-work check (VR-14) groups the file's works.
            var divisionId = await DemandLineWrites.OriginDivisionIdAsync(context, p, ct);
            var parsed = new List<ParsedRow>(dataRows);
            var parseFailures = new RuleFailuresDto();
            for (var r = 2; r <= rowCount; r++)
            {
                var row = await ParseRowAsync(sheet, r, divisionId, ct);
                if (row.Failure is { } f)
                    parseFailures.Failures.Add(f);
                else
                    parsed.Add(row);
            }
            if (parseFailures.Failures.Count > 0)
                return BudgetResponses.RuleFailures(parseFailures); // nothing created, every bad row listed

            // Evaluate all file lines together with the proposal's existing live lines, at moment Upload.
            var existing = await context.DemandLines.AsNoTracking()
                .Where(l => l.ProposalId == p.ProposalId && !l.IsDeleted).ToListAsync(ct);
            var candidates = parsed.Select(x => x.Line).ToList();
            var evaluated = await rules.EvaluateAsync(p, [.. existing, .. candidates], RuleMoment.Upload, actor.OfficeLevel, ct);
            var fileSerials = parsed.Select(x => x.Line.SerialNo).ToHashSet();
            var fileFailures = evaluated.Failures.Where(fl => fl.SerialNo is { } s && fileSerials.Contains(s)).ToList();
            if (fileFailures.Count > 0)
                return BudgetResponses.RuleFailures(new RuleFailuresDto { Failures = fileFailures });

            // Clean: store the file as a scanned T-22 attachment, then write T-22 + T-34 + lines in one scope.
            var attachment = await PrepareAttachmentAsync(request.File, buffer, now, ct);
            if (attachment.Failure is { } attachFailure)
                return attachFailure;
            var row0 = attachment.Row!;
            var storageName = row0.StorageKey.ToString();
            try
            {
                await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
                await DemandLineWrites.LockProposalAsync(context, p.ProposalId, ct);

                context.BudgetAttachments.Add(row0);
                foreach (var pr in parsed.Where(x => x.NewWork is not null))
                    context.Works.Add(pr.NewWork!);
                await context.SaveChangesAsync(ct); // the attachment and the new works exist before the lines reference them

                var nextSerial = (await context.DemandLines.AsNoTracking()
                    .Where(l => l.ProposalId == p.ProposalId).MaxAsync(l => (int?)l.SerialNo, ct) ?? 0) + 1;
                var batch = new UploadBatch
                {
                    ProposalId = p.ProposalId, FileAttachmentId = row0.AttachmentId, LinesCommitted = parsed.Count,
                };
                context.UploadBatches.Add(batch);
                await context.SaveChangesAsync(ct);

                foreach (var pr in parsed)
                {
                    var line = pr.Line;
                    line.ProposalId = p.ProposalId;
                    line.WorkId = pr.NewWork?.Id ?? pr.ExistingWorkId;
                    line.SerialNo = nextSerial++;
                    line.AddedOnReturn = false; // an upload is accepted only in S-01 (DB-11)
                    line.UploadBatchId = batch.UploadBatchId;
                    context.DemandLines.Add(line);
                }
                DemandLineWrites.RenewLease(p, actor, now, ProposalLease.Length(configuration));
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return new Response<UploadResultDto>
                {
                    IsSucced = true,
                    Message = $"{parsed.Count} line(s) uploaded",
                    Data = new UploadResultDto(batch.UploadBatchId, parsed.Count),
                };
            }
            catch
            {
                storage.TryDelete(FileType.BudgetAttachment, storageName);
                throw;
            }
        }

        private sealed record ParsedRow(DemandLine Line, Work? NewWork, int? ExistingWorkId, RuleFailureItemDto? Failure);

        private async Task<ParsedRow> ParseRowAsync(ExcelWorksheet sheet, int r, int? divisionId, CancellationToken ct)
        {
            var codeText = sheet.Cells[r, UploadTemplateColumns.AccountCode].Text?.Trim() ?? string.Empty;
            var nameText = sheet.Cells[r, UploadTemplateColumns.NameOfWork].Text?.Trim() ?? string.Empty;
            var justification = sheet.Cells[r, UploadTemplateColumns.Justification].Text?.Trim();
            var rbe = ReadRupees(sheet, r, UploadTemplateColumns.Rbe);
            var obe = ReadRupees(sheet, r, UploadTemplateColumns.ObeNext);

            var line = new DemandLine
            {
                LineType = DemandLineType.Work,
                SerialNo = r, // the sheet row, used only to name a failure and to group the file's works
                RbeAmount = rbe,
                ObeNextAmount = obe,
                JustificationText = string.IsNullOrWhiteSpace(justification) ? null : justification,
            };

            RuleFailureItemDto Fail(string field, string id) => new(null, r, field, id, BudgetMessages.Format(id));

            if (string.IsNullOrWhiteSpace(nameText))
                return new ParsedRow(line, null, null, Fail("nameOfWork", BudgetMessages.M21));
            if (string.IsNullOrWhiteSpace(codeText))
                return new ParsedRow(line, null, null, Fail("accountCode", BudgetMessages.M05));

            var code = await context.HeadOfAccounts.AsNoTracking()
                .FirstOrDefaultAsync(h => h.CompleteCode == codeText, ct);
            if (code is null)
                return new ParsedRow(line, null, null, Fail("accountCode", BudgetMessages.M05));
            if (divisionId is null)
                return new ParsedRow(line, null, null, Fail("accountCode", BudgetMessages.M05));

            line.AccountCodeId = code.AccountCodeId;
            var work = new Work
            {
                WorkCode = await workRules.NextInterimWorkCodeAsync(divisionId.Value, ct),
                DivisionId = divisionId.Value,
                WorkDesc = nameText.Length > 500 ? nameText[..500] : nameText,
                WorkClass = WorkClass.Aa,
                EnteredVia = WorkEnteredVia.DemandEntry,
                PreDdd = false,
                UserCode = DemandLineWrites.DemandEntryUserCode,
            };
            WorkRules.ApplySegments(work, code);
            return new ParsedRow(line, work, null, null);
        }

        private sealed record AttachmentPrep(BudgetAttachment? Row, Response? Failure);

        private async Task<AttachmentPrep> PrepareAttachmentAsync(
            Microsoft.AspNetCore.Http.IFormFile file, MemoryStream buffer, DateTimeOffset now, CancellationToken ct)
        {
            var extension = AttachmentContent.ExtensionOf(file.FileName);
            if (extension is not ("xlsx" or "xls"))
                return new AttachmentPrep(null, BudgetResponses.Fail(BudgetMessages.M11));

            var head = buffer.GetBuffer().AsSpan(0, (int)Math.Min(HeadLength, buffer.Length));
            var contentType = AttachmentContent.DetectContentType(extension, head);
            if (contentType is null)
                return new AttachmentPrep(null, BudgetResponses.Fail(BudgetMessages.M11));

            buffer.Position = 0;
            var digest = await AttachmentContent.Sha256HexAsync(buffer, ct);
            buffer.Position = 0;
            var scan = await scanner.ScanAsync(buffer, ct);
            switch (scan.Verdict)
            {
                case ScanVerdict.Unavailable:
                    logger.LogError("Upload scanner unavailable ({Engine}): {Detail}", scan.Engine, scan.Detail);
                    return new AttachmentPrep(null, new Response
                    {
                        IsSucced = false, MessageId = BudgetMessages.M24,
                        Message = BudgetMessages.Format(BudgetMessages.M24) + " Malware scanning is unavailable; the file was not accepted.",
                    });
                case ScanVerdict.Infected:
                    logger.LogWarning("Upload refused by malware scan: digest {Digest}, signature {Detail}", digest, scan.Detail);
                    return new AttachmentPrep(null, new Response
                    {
                        IsSucced = false, MessageId = BudgetMessages.M24,
                        Message = BudgetMessages.Format(BudgetMessages.M24) + " The file failed the malware scan.",
                    });
            }

            var row = new BudgetAttachment
            {
                StorageKey = Guid.NewGuid(),
                OriginalFileName = Path.GetFileName(file.FileName),
                FileExtension = extension,
                ContentType = contentType,
                EvidenceClass = AttachmentEvidenceClass.Supporting,
                SizeBytes = buffer.Length,
                Sha256Digest = digest,
                ScanEngine = scan.Engine.Length > 100 ? scan.Engine[..100] : scan.Engine,
                ScannedAt = now,
            };
            buffer.Position = 0;
            await storage.SaveFileAsync(buffer, FileType.BudgetAttachment, row.StorageKey.ToString(), ct);
            return new AttachmentPrep(row, null);
        }

        private static bool HeadingsMatch(ExcelWorksheet sheet)
        {
            var columns = sheet.Dimension?.Columns ?? 0;
            if (columns < UploadTemplateColumns.Headings.Count)
                return false;
            for (var c = 0; c < UploadTemplateColumns.Headings.Count; c++)
            {
                var heading = sheet.Cells[1, c + 1].Text?.Trim() ?? string.Empty;
                if (!string.Equals(heading, UploadTemplateColumns.Headings[c], StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        private static decimal? ReadRupees(ExcelWorksheet sheet, int r, int c)
        {
            var cell = sheet.Cells[r, c];
            if (cell.Value is null)
                return null;
            if (cell.Value is double d)
                return (decimal)d;
            var text = cell.Text?.Trim();
            return decimal.TryParse(text, out var value) ? value : null;
        }

        private static async Task<MemoryStream?> BufferAsync(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length <= 0 || file.Length > AttachmentContent.MaxSizeBytes)
                return null;
            var buffer = new MemoryStream((int)Math.Min(file.Length, AttachmentContent.MaxSizeBytes));
            await using var source = file.OpenReadStream();
            await source.CopyToAsync(buffer, ct);
            if (buffer.Length == 0)
            {
                await buffer.DisposeAsync();
                return null;
            }
            return buffer;
        }
    }
}
