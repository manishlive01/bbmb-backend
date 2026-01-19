using FluentValidation;

namespace BBMB.BudgetService.Features.Projects.Create
{
    public class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectValidator()
        {
            RuleFor(x => x.dto.ProjCode)
                .NotEmpty().WithMessage("Project code is required")
                .MaximumLength(1).WithMessage("Project code must be 1 character");

            RuleFor(x => x.dto.ProjDesc)
                .NotEmpty().WithMessage("Project description is required")
                .MaximumLength(50).WithMessage("Project description must not exceed 50 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
