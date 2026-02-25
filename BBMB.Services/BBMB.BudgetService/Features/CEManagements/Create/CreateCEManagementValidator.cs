using BBMB.BudgetService.Features.CEManagements.Create;
using FluentValidation;

namespace BBMB.BudgetService.Features.CEManagements.Create
{
    public class CreateCEManagementValidator
        : AbstractValidator<CreateCEManagementCommand>
    {
        public CreateCEManagementValidator()
        {
            RuleFor(x => x.dto.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(15).WithMessage("Name must not exceed 15 characters");

            RuleFor(x => x.dto.CECode)
               .NotEmpty().WithMessage("CE code is required");

            RuleFor(x => x.dto.ProjectCode)
                .NotEmpty().WithMessage("Project code is required")
                .MaximumLength(2).WithMessage("Project code must not exceed 2 characters");

            RuleFor(x => x.dto.Description)
                .MaximumLength(100).WithMessage("Description must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.Description));

            RuleFor(x => x.dto.Location)
                .MaximumLength(100).WithMessage("Location must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.Location));
        }
    }
}
