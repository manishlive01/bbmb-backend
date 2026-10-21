using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Entities;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create
{
    /// <summary>
    /// Design §8.4 F-06 validator: tab AA ⇒ adminApprovalId required and workId / newAmrWork absent; tab AMR ⇒
    /// adminApprovalId absent and exactly one of workId / newAmrWork; estimate ≤ 500; TS number ≤ 50; date not after
    /// today IST; amount &gt; 0 whole rupees; copy required (M-07).
    /// </summary>
    public sealed class CreateTechnicalSanctionValidator : AbstractValidator<CreateTechnicalSanctionCommand>
    {
        public CreateTechnicalSanctionValidator(TimeProvider? clock = null)
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.Tab).Must(t => t is SanctionTab.Aa or SanctionTab.Amr).WithMessage("tab must be AA or AMR.");
                When(x => x.Dto.Tab == SanctionTab.Aa, () =>
                {
                    RuleFor(x => x.Dto.AdminApprovalId).NotNull().GreaterThan(0);
                    RuleFor(x => x.Dto.WorkId).Null().WithMessage("On the AA tab the work is the approval's.");
                    RuleFor(x => x.Dto.NewAmrWork).Null().WithMessage("On the AA tab the work is the approval's.");
                });
                When(x => x.Dto.Tab == SanctionTab.Amr, () =>
                {
                    RuleFor(x => x.Dto.AdminApprovalId).Null().WithMessage("An AM&R sanction has no administrative approval.");
                    RuleFor(x => x.Dto).Must(d => (d.WorkId is null) != (d.NewAmrWork is null))
                        .WithMessage("Exactly one of workId and newAmrWork is required on the AMR tab.");
                    RuleFor(x => x.Dto.NewAmrWork!.NameOfWork).Must(t => !string.IsNullOrWhiteSpace(t)).MaximumLength(500)
                        .When(x => x.Dto.NewAmrWork is not null);
                    RuleFor(x => x.Dto.NewAmrWork!.AccountCodeId).GreaterThan(0).When(x => x.Dto.NewAmrWork is not null);
                });
                RuleFor(x => x.Dto.NameOfEstimate).Must(t => !string.IsNullOrWhiteSpace(t)).WithErrorCode(BudgetMessages.M21)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M21)).MaximumLength(500);
                RuleFor(x => x.Dto.TsNumber).Must(t => !string.IsNullOrWhiteSpace(t)).WithErrorCode(BudgetMessages.M21)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M21)).MaximumLength(50);
                RuleFor(x => x.Dto.TsDate).NotEqual(default(DateOnly)).NotAfterTodayIst(clock);
                RuleFor(x => x.Dto.TsAmount).GreaterThan(0).WholeRupees();
                RuleFor(x => x.Dto.TsCopyAttachmentId).NotNull().WithErrorCode(BudgetMessages.M07)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M07));
            });
        }
    }
}
