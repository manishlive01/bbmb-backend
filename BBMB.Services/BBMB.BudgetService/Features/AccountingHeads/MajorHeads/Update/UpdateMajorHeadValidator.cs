using FluentValidation;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update
{
    public sealed class UpdateMajorHeadValidator
        : AbstractValidator<UpdateMajorHeadCommand>
    {
        public UpdateMajorHeadValidator()
        {
            RuleFor(x => x.dto.MajorAccCode)
                .NotEmpty()
                .Length(1, 4)
                .WithMessage("Major Account Code is required and must be maximum 4 characters");

            RuleFor(x => x.dto.MajorAccDesc)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Major Account Description is required and must be maximum 100 characters");

            RuleFor(x => x.dto.UserCode)
                .NotEmpty()
                .Length(1, 6)
                .WithMessage("User Code is required and must be maximum 6 characters");

        }
    }
}
