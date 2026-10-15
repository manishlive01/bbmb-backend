using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.Create
{
    public sealed class CreateBudgetCycleValidator : AbstractValidator<CreateBudgetCycleCommand>
    {
        public CreateBudgetCycleValidator()
        {
            // fa_financial_years identifies a year by its closing year (DDD T-08).
            RuleFor(x => x.Dto.CurrentFinYear).InclusiveBetween(2000, 9998)
                .WithMessage("currentFinYear must be a closing year between 2000 and 9998.");
        }
    }
}
