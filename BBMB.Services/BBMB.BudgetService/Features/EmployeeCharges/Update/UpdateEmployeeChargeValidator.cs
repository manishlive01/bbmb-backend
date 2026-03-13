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

            RuleFor(x => x.dto.CECode)
    .NotNull().WithMessage("CE code is required")
    .GreaterThan(0).WithMessage("CE code must be greater than 0");

            RuleFor(x => x.dto.CRCode)
                .NotNull().WithMessage("CR code is required")
                .GreaterThan(0).WithMessage("CR code must be greater than 0");
            RuleFor(x => x.dto.ChargeTime)
                .NotEmpty();
        }
    }
}