using BBMB.BudgetService.Features.BudgetProcess.Allocations.Entities;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Allocate
{
    /// <summary>
    /// Design §8.6 allocate validator (shape only): body in {BSC, FULL_BOARD}; minutes ref ≤ 50; minutes date
    /// not after today in IST; attachment id positive (M-16 is the evidence-class rule, in the handler); FA&amp;CAO
    /// ref ≤ 100 (BUD-188); remarks ≤ 4000. The preview-match and evidence-class rules are the handler's.
    /// </summary>
    public sealed class AllocateValidator : AbstractValidator<AllocateCommand>
    {
        public AllocateValidator()
        {
            RuleFor(x => x.Dto.Body)
                .Must(b => b is AllocationBody.Bsc or AllocationBody.FullBoard)
                .WithMessage("Body must be BSC or FULL_BOARD.");
            RuleFor(x => x.Dto.MinutesReference)
                .NotEmpty().WithMessage("Minutes reference is required.")
                .MaximumLength(50).WithMessage("Minutes reference must not exceed 50 characters.");
            RuleFor(x => x.Dto.MinutesDate).NotAfterTodayIst();
            RuleFor(x => x.Dto.MinutesAttachmentId)
                .GreaterThan(0).WithMessage("A minutes attachment is required.");
            RuleFor(x => x.Dto.FaCaoApprovalRef)
                .NotEmpty().WithMessage("FA & CAO approval reference is required.")
                .MaximumLength(100).WithMessage("FA & CAO approval reference must not exceed 100 characters.");
            RuleFor(x => x.Dto.Remarks)
                .NotEmpty().WithMessage("Remarks are required.")
                .MaximumLength(4000).WithMessage("Remarks must not exceed 4000 characters.");
        }
    }
}
