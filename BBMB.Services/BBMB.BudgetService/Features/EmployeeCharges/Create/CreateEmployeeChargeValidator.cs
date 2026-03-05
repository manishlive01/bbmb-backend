using FluentValidation;

namespace BBMB.BudgetService.Features.EmployeeCharges.Create
{
    public class CreateEmployeeChargeValidator : AbstractValidator<CreateEmployeeChargeCommand>
    {
        public CreateEmployeeChargeValidator()
        {
            RuleFor(x => x.dto.EmployeeId)
                .NotEmpty();

            RuleFor(x => x.dto.UserId)
                .NotEmpty();

            RuleFor(x => x.dto.ProjectId)
                .GreaterThan(0);

            RuleFor(x => x.dto.CECode)
                .NotEmpty()
                .MaximumLength(10);

            RuleFor(x => x.dto.CRCode)
                .NotEmpty()
                .MaximumLength(10);

            RuleFor(x => x.dto.DivCode)
                .NotEmpty()
                .MaximumLength(10);

            RuleFor(x => x.dto.ChargeTime)
                .NotEmpty();
        }
    }
}
