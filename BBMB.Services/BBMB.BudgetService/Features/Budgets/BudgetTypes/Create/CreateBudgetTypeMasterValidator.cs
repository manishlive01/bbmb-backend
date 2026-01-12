using FluentValidation;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.Create
{
    /// <summary>
    /// Validator for creating BudgetTypeMaster
    /// Runs automatically via MediatR ValidationBehavior
    /// </summary>
    public sealed class CreateBudgetTypeMasterValidator
        : AbstractValidator<CreateBudgetTypeMasterCommand>
    {
        public CreateBudgetTypeMasterValidator()
        {
            // ==========================
            // Budget Type Code Validation
            // ==========================
            RuleFor(x => x.dto.BudgetTypeCode)
                .NotEmpty()
                .WithMessage("Budget type code is required.")
                .Length(2, 10)
                .WithMessage("Budget type code must be between 2 and 10 characters.")
                .Matches("^[A-Z0-9_ ]+$")
                .WithMessage("Budget type code must be uppercase and may contain letters, numbers, or underscore.");

            // ==========================
            // Budget Type Name Validation
            // ==========================
            RuleFor(x => x.dto.BudgetTypeName)
                .NotEmpty()
                .WithMessage("Budget type name is required.")
                .MaximumLength(100)
                .WithMessage("Budget type name cannot exceed 100 characters.")
                .Matches("^[A-Za-z ]+$")
                .WithMessage("Budget type name must contain only alphabet characters.");

            // ==========================
            // Description Validation (Optional)
            // ==========================
            RuleFor(x => x.dto.Description)
                .MaximumLength(250)
                .WithMessage("Description cannot exceed 250 characters.");

            // ==========================
            // IsActive Validation
            // ==========================
            RuleFor(x => x.dto.IsActive)
                .NotNull()
                .WithMessage("IsActive flag must be specified.");
        }
    }
}
