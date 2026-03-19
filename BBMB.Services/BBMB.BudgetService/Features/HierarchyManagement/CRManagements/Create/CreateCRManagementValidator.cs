using FluentValidation;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Create
{
    public class CreateCRManagementValidator
        : AbstractValidator<CreateCRManagementCommand>
    {
        public CreateCRManagementValidator()
        {
            RuleFor(x => x.Dto.CECode)
            .NotNull().WithMessage("CE code is required")
            .GreaterThan(0).WithMessage("CE code must be greater than 0");

            RuleFor(x => x.Dto.CRCode)
                .NotNull().WithMessage("CR code is required")
                .GreaterThan(0).WithMessage("CR code must be greater than 0");

            RuleFor(x => x.Dto.Name)
                .NotEmpty()
                .MaximumLength(15)
                .WithMessage("Name must not exceed 15 characters");

            RuleFor(x => x.Dto.Description)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Dto.Description));

            RuleFor(x => x.Dto.Location)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Dto.Location));
        }
    }
}
