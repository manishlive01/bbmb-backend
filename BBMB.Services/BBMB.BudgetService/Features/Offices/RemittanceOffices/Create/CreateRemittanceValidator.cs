using FluentValidation;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.Create
{
    public class CreateRemittanceValidator : AbstractValidator<CreateRemittanceCommand>
    {
        public CreateRemittanceValidator()
        {
            RuleFor(x => x.dto.RoCode)
                .NotEmpty().WithMessage("Remittance code is required")
                .MaximumLength(3).WithMessage("Remittance code must be maximum 2 characters");

            RuleFor(x => x.dto.RoDesc)
                .NotEmpty().WithMessage("Remittance description is required")
                .MaximumLength(50).WithMessage("Remittance description must not exceed 50 characters");

            //RuleFor(x => x.dto.UserCode)
            //    .NotEmpty().WithMessage("User code is required")
            //    .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
