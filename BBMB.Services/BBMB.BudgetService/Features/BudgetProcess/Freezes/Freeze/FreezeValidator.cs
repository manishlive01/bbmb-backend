using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Freezes.Freeze
{
    /// <summary>Design §8.6 freeze validator (shape): FA&amp;CAO ref ≤ 100 (BUD-192); remarks ≤ 4000.</summary>
    public sealed class FreezeValidator : AbstractValidator<FreezeCommand>
    {
        public FreezeValidator()
        {
            RuleFor(x => x.Dto.FaCaoApprovalRef)
                .NotEmpty().WithMessage("FA & CAO approval reference is required.")
                .MaximumLength(100).WithMessage("FA & CAO approval reference must not exceed 100 characters.");
            RuleFor(x => x.Dto.Remarks)
                .NotEmpty().WithMessage("Remarks are required.")
                .MaximumLength(4000).WithMessage("Remarks must not exceed 4000 characters.");
        }
    }
}
