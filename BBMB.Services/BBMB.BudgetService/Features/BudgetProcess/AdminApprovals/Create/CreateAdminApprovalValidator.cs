using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create
{
    /// <summary>
    /// Design §8.4 F-03 validator: exactly one of workId / newWork (class AA); nameOfWork required ≤ 500; reference
    /// required ≤ 50; date not after today IST (BUD-014); amount &gt; 0 whole rupees; copy required (M-04, VR-10);
    /// eOffice ref ≤ 50; authority in the four (BUD-054); justification ≤ 4000.
    /// </summary>
    public sealed class CreateAdminApprovalValidator : AbstractValidator<CreateAdminApprovalCommand>
    {
        public CreateAdminApprovalValidator(TimeProvider? clock = null)
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto).Must(d => (d.WorkId is null) != (d.NewWork is null))
                    .WithMessage("Exactly one of workId and newWork is required.");
                RuleFor(x => x.Dto.WorkId).GreaterThan(0).When(x => x.Dto.WorkId is not null);
                RuleFor(x => x.Dto.NewWork!.WorkClass).Equal(WorkClass.Aa).When(x => x.Dto.NewWork is not null)
                    .WithMessage("An administrative approval is of an AA work.");
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
