using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Submit
{
    /// <summary>Design §8.7 submit validator (shape): an order attachment id is required; confirm must be true.</summary>
    public sealed class SubmitReappropriationValidator : AbstractValidator<SubmitReappropriationCommand>
    {
        public SubmitReappropriationValidator()
        {
            RuleFor(x => x.Dto.OrderAttachmentId).GreaterThan(0).WithMessage("An order attachment is required.");
            RuleFor(x => x.Dto.Confirm).Equal(true).WithMessage("Confirmation is required to submit.");
        }
    }
}
