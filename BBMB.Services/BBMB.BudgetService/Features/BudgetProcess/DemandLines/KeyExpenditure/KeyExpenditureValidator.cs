using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure
{
    /// <summary>
    /// Design §8.3 KeyExpenditure: nature and period in the T-35 values; upToMonth exactly for UP_TO_MONTH and the
    /// first day of a month (BUD-141); amount whole rupees (BUD-013).
    /// </summary>
    public sealed class KeyExpenditureValidator : AbstractValidator<KeyExpenditureCommand>
    {
        public KeyExpenditureValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.Nature).Must(n => ExpenditureValues.Natures.Contains(n))
                    .WithMessage("nature must be EXPENDITURE or RECEIPT.");
                RuleFor(x => x.Dto.Period).Must(p => ExpenditureValues.Periods.Contains(p))
                    .WithMessage("period must be FULL_YEAR, UP_TO_MONTH or UP_TO_DATE.");
                RuleFor(x => x.Dto.UpToMonth).NotNull().Must(m => m!.Value.Day == 1)
                    .When(x => x.Dto.Period == ExpenditureValues.UpToMonth)
                    .WithMessage("upToMonth is required for UP_TO_MONTH and is the first day of the month.");
                RuleFor(x => x.Dto.UpToMonth).Null()
                    .When(x => x.Dto.Period != ExpenditureValues.UpToMonth)
                    .WithMessage("upToMonth applies to UP_TO_MONTH only.");
                RuleFor(x => x.Dto.Amount).WholeRupees();
            });
        }
    }
}
