using FluentValidation;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Update
{
    public class UpdateDivSectionManagementValidator
        : AbstractValidator<UpdateDivSectionManagementCommand>
    {
        public UpdateDivSectionManagementValidator()
        {
            RuleFor(x => x.dto.Id)
                .GreaterThan(0);

            RuleFor(x => x.dto.DivisionId)
                .GreaterThan(0);

            RuleFor(x => x.dto.DivSectionCode)
                .GreaterThan(0);

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