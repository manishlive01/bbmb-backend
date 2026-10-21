using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update
{
    /// <summary>
    /// Design §8.3 Update validator (shape only, as Add): ids positive, unforeseenName ≤ 500, amounts whole
    /// rupees with negatives allowed (BUD-088), justification ≤ 4000. Which fields a line carries depends on its
    /// stored type, so the handler checks those (M-21 / M-22 on the field).
    /// </summary>
    public sealed class UpdateDemandLineValidator : AbstractValidator<UpdateDemandLineCommand>
    {
        public UpdateDemandLineValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.AccountCodeId).GreaterThan(0).When(x => x.Dto.AccountCodeId is not null);
                RuleFor(x => x.Dto.PrecedingAmrWorkId).GreaterThan(0).When(x => x.Dto.PrecedingAmrWorkId is not null);
                RuleFor(x => x.Dto.UnforeseenName).MaximumLength(500);
                RuleFor(x => x.Dto.RbeAmount).WholeRupees();
                RuleFor(x => x.Dto.ObeNextAmount).WholeRupees();
                RuleFor(x => x.Dto.JustificationText).MaximumLength(4000);
                RuleFor(x => x.Dto.JustificationAttachmentId).GreaterThan(0).When(x => x.Dto.JustificationAttachmentId is not null);
                RuleFor(x => x.Dto.CeCertificateAttachmentId).GreaterThan(0).When(x => x.Dto.CeCertificateAttachmentId is not null);
                RuleFor(x => x.Dto.CeAmrApprovalAttachmentId).GreaterThan(0).When(x => x.Dto.CeAmrApprovalAttachmentId is not null);
            });
        }
    }
}
