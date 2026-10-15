using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure
{
    /// <summary>Design §8.3 KeyOpeningFigure: figureKind in the T-36 values; amount whole rupees (budget.rupees).</summary>
    public sealed class KeyOpeningFigureValidator : AbstractValidator<KeyOpeningFigureCommand>
    {
        public KeyOpeningFigureValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.FigureKind).Must(k => OpeningFigureKind.All.Contains(k))
                    .WithMessage("figureKind must be OBE_CURRENT or RBE_PREVIOUS.");
                RuleFor(x => x.Dto.Amount).WholeRupees();
            });
        }
    }
}
