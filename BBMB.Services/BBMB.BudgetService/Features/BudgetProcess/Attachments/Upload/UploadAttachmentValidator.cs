using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Attachments.Upload
{
    /// <summary>
    /// Shape only (400): a file is present and the evidence class is one of the two T-22 values. Format, size and
    /// content are business refusals answered 409 M-24 by the handler (design §4.8 step 1, §10).
    /// </summary>
    public sealed class UploadAttachmentValidator : AbstractValidator<UploadAttachmentCommand>
    {
        public UploadAttachmentValidator()
        {
            RuleFor(x => x.File).NotNull();
            RuleFor(x => x.EvidenceClass).NotEmpty()
                .Must(c => c is AttachmentEvidenceClass.Evidence or AttachmentEvidenceClass.Supporting)
                .WithMessage("evidenceClass must be EVIDENCE or SUPPORTING.");
        }
    }
}
