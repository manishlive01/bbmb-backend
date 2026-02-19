using FluentValidation;

namespace BBMB.BudgetService.Features.CEManagements.Update
{
    public class UpdateCEManagementValidator
        : AbstractValidator<UpdateCEManagementCommand>
    {
        public UpdateCEManagementValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid ID is required");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(15).WithMessage("Name must not exceed 15 characters");

            RuleFor(x => x.Dto.ProjectCode)
                .NotEmpty().WithMessage("Project code is required")
                .MaximumLength(2).WithMessage("Project code must not exceed 2 characters");

            RuleFor(x => x.Dto.Description)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Dto.Description))
                .WithMessage("Description must not exceed 100 characters");

            RuleFor(x => x.Dto.Location)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Dto.Location))
                .WithMessage("Location must not exceed 100 characters");

            RuleFor(x => x.Dto.Date)
                .LessThanOrEqualTo(DateTime.Now)
                .When(x => x.Dto.Date.HasValue)
                .WithMessage("Date cannot be in the future");

            RuleFor(x => x.Dto.UserCode)
                .NotEmpty().WithMessage("User code is required")
                .MaximumLength(6).WithMessage("User code must not exceed 6 characters");
        }
    }
}
