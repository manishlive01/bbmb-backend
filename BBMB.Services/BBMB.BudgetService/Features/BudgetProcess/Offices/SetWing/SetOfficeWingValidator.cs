using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing
{
    /// <summary>Shape only; whether the wing exists in irr_pwr is the handler's check (M-05).</summary>
    public sealed class SetOfficeWingValidator : AbstractValidator<SetOfficeWingCommand>
    {
        public SetOfficeWingValidator()
        {
            RuleFor(x => x.OfficeId).GreaterThan(0);
            RuleFor(x => x.Dto.WingCode)
                .NotEmpty().WithMessage("wingCode is required.")
                .MaximumLength(2).WithMessage("wingCode must not exceed 2 characters.");
        }
    }
}
