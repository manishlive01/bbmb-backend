using FluentValidation;

namespace BBMB.BudgetService.Features.Designations.Update
{
    public class UpdateDesignationValidator
        : AbstractValidator<UpdateDesignationCommand>
    {
        public UpdateDesignationValidator()
        {
            RuleFor(x => x.Dto.DesignationDesc)
                .NotEmpty().WithMessage("Designation description is required")
                .MaximumLength(50).WithMessage("Designation description must not exceed 50 characters");

            RuleFor(x => x.Dto.ClassCode)
                .NotEmpty().WithMessage("Class code is required")
                .MaximumLength(1).WithMessage("Class code must be 1 character")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Class code must be alphanumeric");

            RuleFor(x => x.Dto.MinBasic)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Dto.MinBasic.HasValue)
                .WithMessage("Minimum basic must be greater than or equal to 0");

            RuleFor(x => x.Dto.MaxBasic)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Dto.MaxBasic.HasValue)
                .WithMessage("Maximum basic must be greater than or equal to 0");

            RuleFor(x => x)
                .Must(x => !x.Dto.MinBasic.HasValue || !x.Dto.MaxBasic.HasValue || x.Dto.MinBasic <= x.Dto.MaxBasic)
                .WithMessage("Minimum basic cannot be greater than maximum basic");
        }
    }
}
