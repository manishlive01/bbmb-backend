using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Recommendations.Upsert
{
    /// <summary>
    /// Design §8.3 Upsert: a head-wise category; ids positive; figures whole rupees; concurrenceRef required
    /// (M-21) where either figure is set (BUD-153; T-37 CHECK) and ≤ 100.
    /// </summary>
    public sealed class UpsertRecommendationValidator : AbstractValidator<UpsertRecommendationCommand>
    {
        public UpsertRecommendationValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.Category).Must(c => ProposalCategory.HeadWise.Contains(c))
                    .WithMessage("category must be DNA, RECEIPTS or CAPEX.");
                RuleFor(x => x.Dto.CeOfficeId).GreaterThan(0);
                RuleFor(x => x.Dto.AccountCodeId).GreaterThan(0);
                RuleFor(x => x.Dto.RbeRecommended).WholeRupees();
                RuleFor(x => x.Dto.ObeNextRecommended).WholeRupees();
                RuleFor(x => x.Dto.ConcurrenceRef).Must(r => !string.IsNullOrWhiteSpace(r))
                    .When(x => x.Dto.RbeRecommended is not null || x.Dto.ObeNextRecommended is not null)
                    .WithErrorCode(BudgetMessages.M21).WithMessage("concurrenceRef is required where a recommended figure is set.");
                RuleFor(x => x.Dto.ConcurrenceRef).MaximumLength(100);
            });
        }
    }
}
