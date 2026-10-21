using FluentValidation;
namespace BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse
{
    /// <summary>Design §8.4: the e-Office reference is required (T-07 CHECK, not_for_use_ref varchar(50)).</summary>
    public sealed class MarkNotForUseValidator : AbstractValidator<MarkNotForUseCommand>
    {
        public MarkNotForUseValidator()
        {
            RuleFor(x => x.AccountCodeId).GreaterThan(0);
            RuleFor(x => x.Dto).NotNull();
            RuleFor(x => x.Dto.EofficeRef).NotEmpty().MaximumLength(50).When(x => x.Dto is not null);
        }
    }
}
