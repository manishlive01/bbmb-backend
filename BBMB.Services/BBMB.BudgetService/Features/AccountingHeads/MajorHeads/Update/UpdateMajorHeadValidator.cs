using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update
{
    public class UpdateMajorHeadValidator
        : AbstractValidator<UpdateMajorHeadDto>
    {
        public UpdateMajorHeadValidator()
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
