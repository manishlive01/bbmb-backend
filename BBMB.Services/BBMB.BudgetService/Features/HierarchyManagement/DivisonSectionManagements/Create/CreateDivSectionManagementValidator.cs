using FluentValidation;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create
{
    public class CreateDivSectionManagementValidator
        : AbstractValidator<CreateDivSectionManagementCommand>
    {
        public CreateDivSectionManagementValidator()
        {
            RuleFor(x => x.dto.DivisionId)
                .GreaterThan(0).WithMessage("DivisionId must be greater than 0");

            RuleFor(x => x.dto.DivSectionCode)
                .GreaterThan(0).WithMessage("DivSectionCode must be greater than 0");

            RuleFor(x => x.dto.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200);

            RuleFor(x => x.dto.Description)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.dto.Description));

            RuleFor(x => x.dto.Location)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.dto.Location));
        }
    }
}