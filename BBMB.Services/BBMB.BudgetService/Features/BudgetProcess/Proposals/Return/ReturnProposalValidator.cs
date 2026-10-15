using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Return
{
    /// <summary>Shape only: remark ≤ 4000; selected ids positive and distinct. Whether a remark is required is T-16's (M-12).</summary>
    public sealed class ReturnProposalValidator : AbstractValidator<ReturnProposalCommand>
    {
        public ReturnProposalValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            RuleFor(x => x.Dto.Remark).MaximumLength(4000).When(x => x.Dto is not null);
            RuleForEach(x => x.Dto.SelectedDemandLineIds).GreaterThan(0).When(x => x.Dto?.SelectedDemandLineIds is not null);
            RuleFor(x => x.Dto.SelectedDemandLineIds).Must(ids => ids!.Distinct().Count() == ids!.Count)
                .When(x => x.Dto?.SelectedDemandLineIds is not null).WithMessage("selectedDemandLineIds may name a line once.");
        }
    }
}
