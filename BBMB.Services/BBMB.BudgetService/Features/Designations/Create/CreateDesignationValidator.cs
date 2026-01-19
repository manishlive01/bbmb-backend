using BBMB.BudgetService.Features.Designations.Create.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.Designations.Create
{
    public class CreateDesignationValidator : AbstractValidator<CreateDesignationDto>
    {
        public CreateDesignationValidator()
        {
            RuleFor(x => x.DesignationCode)
                .NotEmpty().WithMessage("Designation code is required")
                .MaximumLength(4).WithMessage("Designation code must not exceed 4 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Designation code must be alphanumeric");

            RuleFor(x => x.DesignationDesc)
                .NotEmpty().WithMessage("Designation description is required")
                .MaximumLength(50).WithMessage("Designation description must not exceed 50 characters");

            RuleFor(x => x.ClassCode)
                .NotEmpty().WithMessage("Class code is required")
                .MaximumLength(1).WithMessage("Class code must be 1 character")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Class code must be alphanumeric");

            RuleFor(x => x.MinBasic)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinBasic.HasValue)
                .WithMessage("Minimum basic must be greater than or equal to 0");

            RuleFor(x => x.MaxBasic)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxBasic.HasValue)
                .WithMessage("Maximum basic must be greater than or equal to 0");

            RuleFor(x => x)
                .Must(x => !x.MinBasic.HasValue || !x.MaxBasic.HasValue || x.MinBasic <= x.MaxBasic)
                .WithMessage("Minimum basic cannot be greater than maximum basic");
        }
    }
}
