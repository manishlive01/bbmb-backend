using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Authorise
{
    /// <summary>Design §8.6 authorise validator (shape): reasons required, ≤ 4000.</summary>
    public sealed class AuthoriseUnforeseenValidator : AbstractValidator<AuthoriseUnforeseenCommand>
    {
        public AuthoriseUnforeseenValidator()
        {
            RuleFor(x => x.Dto.Reasons)
                .NotEmpty().WithMessage("Reasons are required.")
                .MaximumLength(4000).WithMessage("Reasons must not exceed 4000 characters.");
        }
    }
}
