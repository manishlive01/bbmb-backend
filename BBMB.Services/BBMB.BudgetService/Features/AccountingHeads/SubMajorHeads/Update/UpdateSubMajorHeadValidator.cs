using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update
{
    public class UpdateSubMajorHeadValidator
        : AbstractValidator<UpdateSubMajorHeadDto>
    {
        public UpdateSubMajorHeadValidator()
        {
            RuleFor(x => x.MajorAccCode)
                .NotEmpty().WithMessage("Major Account Code is required")
                .MaximumLength(4).WithMessage("Major Account Code must not exceed 4 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Major Account Code must be alphanumeric");

            RuleFor(x => x.SubMajorAccCode)
                .NotEmpty().WithMessage("Sub Major Account Code is required")
                .MaximumLength(6).WithMessage("Sub Major Account Code must not exceed 6 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Sub Major Account Code must be alphanumeric");

            RuleFor(x => x.SubMajorAccDesc)
                .NotEmpty().WithMessage("Sub Major Description is required")
                .MaximumLength(100).WithMessage("Sub Major Description must not exceed 100 characters");
        }
    }
}
