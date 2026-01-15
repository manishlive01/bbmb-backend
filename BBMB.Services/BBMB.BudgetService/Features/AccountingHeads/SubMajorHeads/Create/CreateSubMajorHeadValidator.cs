using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create.Dtos;
using FluentValidation;
using System.Text.RegularExpressions;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create
{
    public class CreateSubMajorHeadValidator : AbstractValidator<CreateSubMajorHeadDto>
    {
        public CreateSubMajorHeadValidator()
        {
            RuleFor(x => x.MajorAccCode)
                .NotEmpty().WithMessage("Major Account Code is required")
                .MaximumLength(4).WithMessage("Major Account Code must be at most 4 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Major Account Code must be alphanumeric");

            RuleFor(x => x.SubMajorAccCode)
                .NotEmpty().WithMessage("Sub Major Account Code is required")
                .MaximumLength(6).WithMessage("Sub Major Account Code must be at most 6 characters")
                .Matches("^[a-zA-Z0-9]+$")
                .WithMessage("Sub Major Account Code must be alphanumeric");

            RuleFor(x => x.SubMajorAccDesc)
                .NotEmpty().WithMessage("Sub Major Description is required")
                .MaximumLength(100).WithMessage("Sub Major Description must not exceed 100 characters");
        }
    }
}
