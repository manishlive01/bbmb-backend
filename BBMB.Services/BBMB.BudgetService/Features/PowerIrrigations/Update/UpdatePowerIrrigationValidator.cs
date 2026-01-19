using FluentValidation;

namespace BBMB.BudgetService.Features.PowerIrrigations.Update
{
    public class UpdatePowerIrrigationValidator : AbstractValidator<UpdatePowerIrrigationCommand>
    {
        public UpdatePowerIrrigationValidator()
        {
            RuleFor(x => x.dto.IrrPwrDesc)
                .NotEmpty().WithMessage("Power/Irrigation description is required")
                .MaximumLength(50).WithMessage("Power/Irrigation description must not exceed 50 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
