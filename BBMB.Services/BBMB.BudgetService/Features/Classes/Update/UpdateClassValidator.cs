using FluentValidation;

namespace BBMB.BudgetService.Features.Classes.Update
{
    public class UpdateClassValidator : AbstractValidator<UpdateClassCommand>
    {
        public UpdateClassValidator()
        {
            RuleFor(x => x.dto.ClassDesc)
                .NotEmpty().WithMessage("Class description is required")
                .MaximumLength(50).WithMessage("Class description must not exceed 50 characters");
        }
    }
}
