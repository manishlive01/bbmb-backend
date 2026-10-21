using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory
{
    /// <summary>The category is one of the four (DDD T-29); RECEIPTS and CAPEX proposals exist from P3.</summary>
    public sealed class FinaliseCategoryValidator : AbstractValidator<FinaliseCategoryCommand>
    {
        public FinaliseCategoryValidator()
        {
            RuleFor(x => x.Category).Must(c => ProposalCategory.All.Contains(c))
                .WithMessage("category must be WORKS, DNA, RECEIPTS or CAPEX.");
        }
    }
}
