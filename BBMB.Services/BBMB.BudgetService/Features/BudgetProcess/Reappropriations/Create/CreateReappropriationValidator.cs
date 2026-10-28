using BBMB.BudgetService.Shared.Validators;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Create
{
    /// <summary>
    /// Design §8.7 F-21 validator (shape): exactly one destination; additional amount &gt; 0 and whole rupees;
    /// e-Office ref ≤ 100 (BUD-205); remarks required ≤ 4000.
    /// </summary>
    public sealed class CreateReappropriationValidator : AbstractValidator<CreateReappropriationCommand>
    {
        public CreateReappropriationValidator()
        {
            RuleFor(x => x.Dto)
                .Must(d => (d.DestinationProvisionId is not null) ^ (d.DestinationNewWorkId is not null))
                .WithMessage("Exactly one destination (an existing provision or a new work) is required.");
            RuleFor(x => x.Dto.AdditionalAmount).GreaterThan(0).WithMessage("Additional amount must be positive.");
            RuleFor(x => x.Dto.AdditionalAmount).WholeRupees();
            RuleFor(x => x.Dto.EofficeOrderRef)
                .NotEmpty().WithMessage("e-Office order reference is required.")
                .MaximumLength(100).WithMessage("e-Office order reference must not exceed 100 characters.");
            RuleFor(x => x.Dto.Remarks)
                .NotEmpty().WithMessage("Remarks are required.")
                .MaximumLength(4000).WithMessage("Remarks must not exceed 4000 characters.");
        }
    }
}
