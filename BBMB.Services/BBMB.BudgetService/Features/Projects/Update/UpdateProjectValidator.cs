using FluentValidation;

namespace BBMB.BudgetService.Features.Projects.Update
{
    public class UpdateProjectValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectValidator()
        {
            RuleFor(x => x.dto.ProjDesc)
                .NotEmpty().WithMessage("Project description is required")
                .MaximumLength(50).WithMessage("Project description must not exceed 50 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
