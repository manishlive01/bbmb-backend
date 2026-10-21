using System.Text.Json.Serialization;
using BBMB.BudgetService.Shared.ResponseDTOs;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload.Dtos
{
    /// <summary>Design §8.2 <c>POST /api/budget-attachments</c> result (§4.8 step 7).</summary>
    public sealed class UploadAttachmentResponseDto
    {
        public long AttachmentId { get; set; }
        /// <summary>Lower-case hex SHA-256 of the stored bytes, as held in T-22 <c>sha256_digest</c>.</summary>
        public string Sha256 { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
    }

    /// <summary>
    /// The upload response. <see cref="ScannerUnavailable"/> is not serialised: it tells the endpoint to answer
    /// 503 (design §10 "Scanner unreachable or not configured") instead of the 409 of the other M-24 failures.
    /// </summary>
    public sealed class UploadAttachmentResponse : Response<UploadAttachmentResponseDto>
    {
        [JsonIgnore]
        public bool ScannerUnavailable { get; init; }
    }
}
