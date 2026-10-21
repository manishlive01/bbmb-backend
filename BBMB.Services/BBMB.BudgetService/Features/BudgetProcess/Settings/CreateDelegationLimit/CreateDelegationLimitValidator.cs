using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Settings.CreateDelegationLimit
{
    /// <summary>
    /// Design §8.2 delegation row: level in the four of AAP-001; maxAmount &gt; 0 and whole rupees where set (T-19
    /// CHECK); effectiveFrom given; sourceRef required, ≤ 100. A past effectiveFrom is accepted: orders of
    /// delegation already in force (R-06, OI-33) are loaded with their own dates.
    /// </summary>
    public sealed class CreateDelegationLimitValidator : AbstractValidator<CreateDelegationLimitCommand>
    {
        public CreateDelegationLimitValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.AuthorityLevel).Must(l => CompetentAuthority.All.Contains(l))
                    .WithMessage($"authorityLevel must be one of {string.Join(", ", CompetentAuthority.All)}.");
                RuleFor(x => x.Dto.MaxAmount).GreaterThan(0).WholeRupees().When(x => x.Dto.MaxAmount is not null);
                RuleFor(x => x.Dto.EffectiveFrom).NotEqual(default(DateOnly)).WithMessage("effectiveFrom is required.");
                RuleFor(x => x.Dto.SourceRef).NotEmpty().MaximumLength(100);
            });
        }
    }
}
