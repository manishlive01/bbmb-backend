using FluentValidation;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update
{
    public class UpdateCRSectionManagementValidator
        : AbstractValidator<UpdateCRSectionManagementCommand>
    {
        public UpdateCRSectionManagementValidator()
        {
            RuleFor(x => x.dto.Id)
                .GreaterThan(0)
                .WithMessage("Id is required");

            RuleFor(x => x.dto.CRId)
                .GreaterThan(0)
                .WithMessage("CRId must be greater than 0");
            RuleFor(x => x.dto.CRSectionCode)
                .GreaterThan(0)
                .NotEmpty()
                .NotNull()
                .WithMessage("Code must be greater than 0");

            RuleFor(x => x.dto.Name)
                .NotEmpty()
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