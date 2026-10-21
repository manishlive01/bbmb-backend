using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures
{
    /// <summary>Shape: amounts whole rupees, negatives allowed (BUD-088); justification ≤ 4000 (T-31).</summary>
    public sealed class AlterFiguresValidator : AbstractValidator<AlterFiguresCommand>
    {
        public AlterFiguresValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.RbeAmount).WholeRupees();
                RuleFor(x => x.Dto.ObeNextAmount).WholeRupees();
                RuleFor(x => x.Dto.Justification).MaximumLength(4000);
            });
        }
    }
}
