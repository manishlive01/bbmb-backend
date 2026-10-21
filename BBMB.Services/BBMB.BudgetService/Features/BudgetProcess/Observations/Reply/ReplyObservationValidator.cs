using BBMB.BudgetService.Shared.Messages;
using FluentValidation;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Reply
{
    /// <summary>Reply text non-blank (M-21) and ≤ 4000 (T-32).</summary>
    public sealed class ReplyObservationValidator : AbstractValidator<ReplyObservationCommand>
    {
        public ReplyObservationValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            RuleFor(x => x.Dto.ReplyText)
                .Must(t => !string.IsNullOrWhiteSpace(t)).WithErrorCode(BudgetMessages.M21).WithMessage("replyText is required.")
                .MaximumLength(4000)
                .When(x => x.Dto is not null);
        }
    }
}
