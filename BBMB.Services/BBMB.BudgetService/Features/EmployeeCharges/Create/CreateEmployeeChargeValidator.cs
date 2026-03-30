using FluentValidation;

namespace BBMB.BudgetService.Features.EmployeeCharges.Create
{
    public class CreateEmployeeChargeValidator : AbstractValidator<CreateEmployeeChargeCommand>
    {
        public CreateEmployeeChargeValidator()
        {
            RuleFor(x => x.dto.EmployeeId)
                .NotEmpty();
             

            RuleFor(x => x.dto.ChargeTime)
                .NotEmpty();
        }
    }
}
