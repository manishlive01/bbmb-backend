using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise
{
    /// <summary>Design §8.4 F-04: "as Create" for the particulars of the version.</summary>
    public sealed class ReviseAdminApprovalValidator : AbstractValidator<ReviseAdminApprovalCommand>
    {
        public ReviseAdminApprovalValidator(TimeProvider? clock = null)
        {
            RuleFor(x => x.AdminApprovalId).GreaterThan(0);
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.NameOfWork).Must(t => !string.IsNullOrWhiteSpace(t)).WithErrorCode(BudgetMessages.M21)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M21)).MaximumLength(500);
                RuleFor(x => x.Dto.AaReferenceNo).Must(t => !string.IsNullOrWhiteSpace(t)).WithErrorCode(BudgetMessages.M21)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M21)).MaximumLength(50);
                RuleFor(x => x.Dto.DateOfApproval).NotEqual(default(DateOnly)).NotAfterTodayIst(clock);
                RuleFor(x => x.Dto.AccountCodeId).GreaterThan(0);
                RuleFor(x => x.Dto.AmountSanctioned).GreaterThan(0).WholeRupees();
                RuleFor(x => x.Dto.AaCopyAttachmentId).NotNull().WithErrorCode(BudgetMessages.M04)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M04));
                RuleFor(x => x.Dto.EofficeRef).MaximumLength(50);
                RuleFor(x => x.Dto.CompetentAuthority).Must(a => CompetentAuthority.All.Contains(a))
                    .WithMessage($"competentAuthority must be one of {string.Join(", ", CompetentAuthority.All)}.");
                RuleFor(x => x.Dto.DelegationJustification).MaximumLength(4000);
            });
        }
    }
}
