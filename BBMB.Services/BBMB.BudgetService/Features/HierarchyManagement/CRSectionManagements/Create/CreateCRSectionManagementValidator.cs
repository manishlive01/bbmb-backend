using FluentValidation;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create
{
    public class CreateCRSectionManagementValidator
        : AbstractValidator<CreateCRSectionManagementCommand>
    {
        public CreateCRSectionManagementValidator()
        {
            RuleFor(x => x.dto.CRId)
                .GreaterThan(0)
                .WithMessage("CRId must be greater than 0");

            RuleFor(x => x.dto.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

            RuleFor(x => x.dto.Description)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.dto.Description));

            RuleFor(x => x.dto.Location)
                .MaximumLength(200)
                .When(x => !string.IsNullOrEmpty(x.dto.Location));
        }
    }
}