using FluentValidation;

namespace BBMB.BudgetService.Features.Classes.Create
{
    public class CreateClassValidator : AbstractValidator<CreateClassCommand>
    {
        public CreateClassValidator()
        {
            RuleFor(x => x.dto.ClassCode)
                .NotEmpty().WithMessage("Class code is required")
                .MaximumLength(1).WithMessage("Class code must be 1 character");

            RuleFor(x => x.dto.ClassDesc)
                .NotEmpty().WithMessage("Class description is required")
                .MaximumLength(50).WithMessage("Class description must not exceed 50 characters");
        }
    }
}
