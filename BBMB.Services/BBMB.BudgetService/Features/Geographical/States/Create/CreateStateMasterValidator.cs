using FluentValidation;

namespace BBMB.BudgetService.Features.Geographical.States.Create
{
    /// <summary>
    /// Validator for creating StateMaster
    /// Runs automatically via MediatR ValidationBehavior
    /// </summary>
    public sealed class CreateStateMasterValidator
        : AbstractValidator<CreateStateMasterCommand>
    {
        public CreateStateMasterValidator()
        {
            // ==========================
            // State Code Validation
            // ==========================
            RuleFor(x => x.dto.StateCode)
                .NotEmpty()
                .WithMessage("State code is required.")
                .Length(2, 5)
                .WithMessage("State code must be between 2 and 5 characters.")
                .WithMessage("State code must contain only uppercase letters.");

            // ==========================
            // State Name Validation
            // ==========================
            RuleFor(x => x.dto.StateName)
                .NotEmpty()
                .WithMessage("State name is required.")
                .MaximumLength(100)
                .WithMessage("State name cannot exceed 100 characters.")
                .Matches("^[A-Za-z ]+$")
                .WithMessage("State name must contain only alphabet.");

            // ==========================
            // IsActive Validation
            // ==========================
            RuleFor(x => x.dto.IsActive)
                .NotNull()
                .WithMessage("IsActive flag must be specified.");
        }
    }
}
