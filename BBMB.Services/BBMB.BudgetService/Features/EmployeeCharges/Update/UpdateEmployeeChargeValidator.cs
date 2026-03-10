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