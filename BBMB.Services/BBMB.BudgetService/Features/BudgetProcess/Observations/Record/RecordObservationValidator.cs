using BBMB.BudgetService.Shared.Messages;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Record
{
    /// <summary>Design §8.3 Record: text non-blank (M-12, BUD-120; T-32 CHECK) and ≤ 4000; line id positive where given.</summary>
    public sealed class RecordObservationValidator : AbstractValidator<RecordObservationCommand>
    {
        public RecordObservationValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.ObservationText)
                    .Must(t => !string.IsNullOrWhiteSpace(t)).WithErrorCode(BudgetMessages.M12)
                    .WithMessage(BudgetMessages.Format(BudgetMessages.M12))
                    .MaximumLength(4000);
                RuleFor(x => x.Dto.DemandLineId).GreaterThan(0).When(x => x.Dto.DemandLineId is not null);
            });
        }
    }
}
