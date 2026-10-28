using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ReferBack
{
    /// <summary>Shape only: remark ≤ 4000 (T-33). The remark-required rule is T-16 <c>remark_required</c> (M-12).</summary>
    public sealed class ReferBackValidator : AbstractValidator<ReferBackCommand>
    {
        public ReferBackValidator()
        {
            RuleFor(x => x.Remark).MaximumLength(4000);
        }
    }
}
