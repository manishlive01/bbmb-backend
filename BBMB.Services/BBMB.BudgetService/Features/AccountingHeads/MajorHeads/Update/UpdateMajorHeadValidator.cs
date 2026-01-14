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
                .NotEmpty()
                .Length(1, 4)
                .WithMessage("Major Account Code is required and must be maximum 4 characters");

            RuleFor(x => x.MajorAccDesc)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Major Account Description is required and must be maximum 100 characters");

            RuleFor(x => x.UserCode)
                .NotEmpty()
                .Length(1, 6)
                .WithMessage("User Code is required and must be maximum 6 characters");

        }
    }
}
