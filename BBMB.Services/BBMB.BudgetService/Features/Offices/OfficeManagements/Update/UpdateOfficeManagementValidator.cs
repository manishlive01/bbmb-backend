using FluentValidation;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Update
{
    public class UpdateOfficeManagementValidator : AbstractValidator<UpdateOfficeManagementCommand>
    {
        public UpdateOfficeManagementValidator()
        {
            RuleFor(x => x.dto.OfficeCode)
                .NotEmpty()
                .MaximumLength(2);

            RuleFor(x => x.dto.OfficeDesc)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.dto.CECode)
                .NotEmpty();

            RuleFor(x => x.dto.CRCode)
                .GreaterThanOrEqualTo(0)
                .When(x => x.dto.CRCode.HasValue);

            RuleFor(x => x.dto.DivisionCode)
                .GreaterThanOrEqualTo(0)
                .When(x => x.dto.DivisionCode.HasValue);
        }
    }
}
