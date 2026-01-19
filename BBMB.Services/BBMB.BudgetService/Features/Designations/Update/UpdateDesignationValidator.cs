using FluentValidation;

namespace BBMB.BudgetService.Features.Designations.Update
{
    public class UpdateDesignationValidator : AbstractValidator<UpdateDesignationCommand>
    {
        public UpdateDesignationValidator()
        {
            RuleFor(x => x.dto.DesignationDesc)
                .NotEmpty().WithMessage("Designation description is required")
                .MaximumLength(50).WithMessage("Designation description must not exceed 50 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
