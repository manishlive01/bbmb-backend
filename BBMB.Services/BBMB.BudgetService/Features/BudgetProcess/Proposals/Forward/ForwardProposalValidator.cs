using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Forward
{
    /// <summary>Shape only: remark ≤ 4000 (T-33). Whether a remark is required is T-16 <c>remark_required</c> (M-12).</summary>
    public sealed class ForwardProposalValidator : AbstractValidator<ForwardProposalCommand>
    {
        public ForwardProposalValidator()
        {
            RuleFor(x => x.Remark).MaximumLength(4000);
        }
    }
}
