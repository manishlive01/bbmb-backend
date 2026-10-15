using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.SetExpenditureSource
{
    /// <summary>Design §8.2: ids positive; mode KEYED or DRAWN (T-18 CHECK). BUD-312 (DRAWN while the stub) is the handler's.</summary>
    public sealed class SetExpenditureSourceValidator : AbstractValidator<SetExpenditureSourceCommand>
    {
        public SetExpenditureSourceValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.OfficeId).GreaterThan(0);
                RuleFor(x => x.Dto.FinYear).GreaterThan(0);
                RuleFor(x => x.Dto.Mode)
                    .Must(m => m == ExpenditureSourceMode.Keyed || m == ExpenditureSourceMode.Drawn)
                    .WithMessage("mode must be KEYED or DRAWN.");
            });
        }
    }
}
