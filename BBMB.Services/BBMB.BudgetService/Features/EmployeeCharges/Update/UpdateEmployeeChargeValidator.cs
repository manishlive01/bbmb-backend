using FluentValidation;
using BBMB.BudgetService.Features.EmployeeCharges.Update.Dtos;

namespace BBMB.BudgetService.Features.EmployeeCharges.Update
{
    public class UpdateEmployeeChargeValidator : AbstractValidator<UpdateEmployeeChargeCommand>
    {
        public UpdateEmployeeChargeValidator()
        {
             RuleFor(x => x.dto.EmployeeId)
                .NotEmpty();

           
            RuleFor(x => x.dto.ChargeTime)
                .NotEmpty();
        }
    }
}