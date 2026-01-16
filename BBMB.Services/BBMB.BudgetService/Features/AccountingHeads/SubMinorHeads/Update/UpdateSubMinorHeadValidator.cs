using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update
{
    public class UpdateSubMinorHeadValidator : AbstractValidator<UpdateSubMinorHeadDto>
    {
        public UpdateSubMinorHeadValidator()
        {
            RuleFor(x => x.MinorAccCode)
                .NotEmpty().WithMessage("Minor Account Code is required")
                .MaximumLength(3).WithMessage("Minor Account Code must be at most 3 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Minor Account Code must be alphanumeric");

            RuleFor(x => x.SubMinorAccCode)
                .NotEmpty().WithMessage("Sub Minor Account Code is required")
                .MaximumLength(3).WithMessage("Sub Minor Account Code must be at most 3 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Sub Minor Account Code must be alphanumeric");

            RuleFor(x => x.SubMinorAccDesc)
                .NotEmpty().WithMessage("Sub Minor Account Description is required")
                .MaximumLength(100).WithMessage("Sub Minor Account Description must not exceed 100 characters");
        }
    }
}
