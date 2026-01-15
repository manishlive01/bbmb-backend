using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create
{
    public class CreateMajorHeadValidator
        : AbstractValidator<CreateMajorHeadDto>
    {
        public CreateMajorHeadValidator()
        {
            RuleFor(x => x.MajorAccCode)
                .NotEmpty().WithMessage("Major Account Code is required")
                .MaximumLength(4).WithMessage("Major Account Code must be at most 4 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Major Account Code must be alphanumeric");

            RuleFor(x => x.MajorAccDesc)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Major Account Description is required and must be maximum 100 characters");


        }
    }
}
