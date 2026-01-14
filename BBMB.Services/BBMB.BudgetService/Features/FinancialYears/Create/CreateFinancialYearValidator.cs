using BBMB.BudgetService.Features.FinancialYears.Create.Dtos;
using FluentValidation;

namespace BBMB.BudgetService.Features.FinancialYears.Create
{
    public class CreateFinancialYearValidator
        : AbstractValidator<CreateFinancialYearDto>
    {
        public CreateFinancialYearValidator()
        {
            RuleFor(x => x.FinYearClosing)
                .NotEmpty()
                .GreaterThan(2000)
                .WithMessage("Financial year closing must be a valid year");

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .WithMessage("End date is required")
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be greater than start date");

            RuleFor(x => x)
                .Must(x => x.StartDate.Year + 1 == x.EndDate.Year)
                .WithMessage("Financial year must span exactly one year");

            RuleFor(x => x.Closed)
                .Must((dto, closedDate) =>
                    closedDate == null || closedDate >= dto.EndDate)
                .WithMessage("Closed date cannot be earlier than end date");
        }
    }
}
