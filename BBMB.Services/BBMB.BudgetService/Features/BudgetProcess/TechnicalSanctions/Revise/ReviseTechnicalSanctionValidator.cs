using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise
{
    /// <summary>Design §8.4 F-06 revision: "as Create" for the version particulars.</summary>
    public sealed class ReviseTechnicalSanctionValidator : AbstractValidator<ReviseTechnicalSanctionCommand>
    {
        public ReviseTechnicalSanctionValidator(TimeProvider? clock = null)
        {
            RuleFor(x => x.TechnicalSanctionId).GreaterThan(0);
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
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
