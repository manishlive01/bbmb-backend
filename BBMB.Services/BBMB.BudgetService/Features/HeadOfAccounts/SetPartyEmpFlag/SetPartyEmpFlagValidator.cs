using FluentValidation;
namespace BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag
{
    /// <summary>The flag is Y or N (T-07 CHECK).</summary>
    public sealed class SetPartyEmpFlagValidator : AbstractValidator<SetPartyEmpFlagCommand>
    {
        public SetPartyEmpFlagValidator()
        {
            RuleFor(x => x.AccountCodeId).GreaterThan(0);
            RuleFor(x => x.Dto).NotNull();
            RuleFor(x => x.Dto.Value).Must(v => v is "Y" or "N").WithMessage("value must be Y or N.").When(x => x.Dto is not null);
        }
    }
}
