using FluentValidation;

namespace BBMB.BudgetService.Features.ObjectClassifications.Update
{
    public class UpdateObjectClassificationValidator : AbstractValidator<UpdateObjectClassificationCommand>
    {
        public UpdateObjectClassificationValidator()
        {
            RuleFor(x => x.dto.ObjectAccDesc)
                .NotEmpty().WithMessage("Object classification description is required")
                .MaximumLength(100).WithMessage("Object classification description must not exceed 100 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
