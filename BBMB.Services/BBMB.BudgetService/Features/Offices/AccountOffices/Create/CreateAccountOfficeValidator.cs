using BBMB.BudgetService.Features.Offices.AccountOffices.Create.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.Offices.AccountOffices.Create
{
    public class CreateAccountOfficeValidator
        : AbstractValidator<CreateAccountOfficeCommand>
    {
        public CreateAccountOfficeValidator()
        {
            RuleFor(x => x.dto.AccOffice)
                .NotEmpty()
                    .WithMessage("Account Office code is required")
                .Length(2)
                    .WithMessage("Account Office code must be exactly 2 characters");

            RuleFor(x => x.dto.AccOfficeDesc)
                .NotEmpty()
                    .WithMessage("Account Office description is required")
                .MaximumLength(50)
                    .WithMessage("Account Office description must not exceed 50 characters");
        }
    }
}
