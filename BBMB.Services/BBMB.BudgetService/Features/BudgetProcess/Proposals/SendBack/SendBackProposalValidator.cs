using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.SendBack
{
    /// <summary>Shape only: remark ≤ 4000 (T-33). Whether a remark is required is T-16 <c>remark_required</c> (M-12).</summary>
    public sealed class SendBackProposalValidator : AbstractValidator<SendBackProposalCommand>
    {
        public SendBackProposalValidator()
        {
            RuleFor(x => x.Remark).MaximumLength(4000);
        }
    }
}
