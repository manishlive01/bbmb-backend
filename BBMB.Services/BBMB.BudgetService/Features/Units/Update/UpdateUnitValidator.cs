using FluentValidation;

namespace BBMB.BudgetService.Features.Units.Update
{
    public class UpdateUnitValidator : AbstractValidator<UpdateUnitCommand>
    {
        public UpdateUnitValidator()
        {
            RuleFor(x => x.dto.UnitDesc)
                .NotEmpty().WithMessage("Unit description is required")
                .MaximumLength(50).WithMessage("Unit description must not exceed 50 characters");

            RuleFor(x => x.dto.ProjCode)
                .NotEmpty().WithMessage("Project code is required")
                .MaximumLength(1).WithMessage("Project code must not exceed 1 character");

            RuleFor(x => x.dto.IrrPwrCode)
                .NotEmpty().WithMessage("Power/Irrigation code is required")
                .MaximumLength(2).WithMessage("Power/Irrigation code must not exceed 2 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
