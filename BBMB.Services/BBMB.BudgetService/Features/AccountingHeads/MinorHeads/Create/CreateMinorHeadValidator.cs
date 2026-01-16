using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create
{
    public class CreateMinorHeadValidator : AbstractValidator<CreateMinorHeadDto>
    {
        public CreateMinorHeadValidator()
        {
            RuleFor(x => x.MinorAccCode)
                .NotEmpty().WithMessage("Minor Account Code is required")
                .MaximumLength(3).WithMessage("Minor Account Code must be at most 3 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Minor Account Code must be alphanumeric");

            RuleFor(x => x.MinorAccDesc)
                .NotEmpty().WithMessage("Minor Account Description is required")
                .MaximumLength(100).WithMessage("Minor Account Description must not exceed 100 characters");

        }
    }
}
