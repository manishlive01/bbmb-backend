using BBMB.BudgetService.Shared.Validators;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.Save
{
    /// <summary>
    /// Design §8.5 Save: a recommendation id; at least one line; each office positive and distinct; amounts
    /// whole rupees and &gt;= 0 (T-39 CHECK). The ceiling (VR-07, M-14) is the handler/DB rule.
    /// </summary>
    public sealed class SaveDistributionValidator : AbstractValidator<SaveDistributionCommand>
    {
        public SaveDistributionValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.RecommendationId).GreaterThan(0);
                RuleFor(x => x.Dto.Lines).NotEmpty().WithMessage("At least one distribution line is required.");
                RuleFor(x => x.Dto.Lines)
                    .Must(lines => lines.Select(l => l.OfficeId).Distinct().Count() == lines.Count)
                    .WithMessage("An office may appear only once in a distribution.");
                RuleForEach(x => x.Dto.Lines).ChildRules(l =>
                {
                    l.RuleFor(y => y.OfficeId).GreaterThan(0);
                    l.RuleFor(y => y.RbeAmount).GreaterThanOrEqualTo(0).WholeRupees();
                    l.RuleFor(y => y.ObeNextAmount).GreaterThanOrEqualTo(0).WholeRupees();
                });
            });
        }
    }
}
