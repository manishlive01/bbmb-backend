using FluentValidation;

namespace BBMB.BudgetService.Features.FinancialYears.Create
{
    public class CreateFinancialYearValidator
        : AbstractValidator<CreateFinancialYearCommand>
    {
        public CreateFinancialYearValidator()
        {
            RuleFor(x => x.dto.FinYearClosing)
                .NotEmpty()
                .GreaterThan(2000)
                .WithMessage("Financial year closing must be a valid year");

            RuleFor(x => x.dto.StartDate)
                .NotEmpty()
                .WithMessage("Start date is required");

            RuleFor(x => x.dto.EndDate)
                .NotEmpty()
                .WithMessage("End date is required")
                .GreaterThan(x => x.dto.StartDate)
                .WithMessage("End date must be greater than start date");

            RuleFor(x => x.dto)
                .Must(x => x.StartDate.Year + 1 == x.EndDate.Year)
                .WithMessage("Financial year must span exactly one year");

            RuleFor(x => x.dto.Closed)
                .Must((cmd, closedDate) =>
                    closedDate == null || closedDate >= cmd.dto.EndDate)
                .WithMessage("Closed date cannot be earlier than end date");
        }
    }
}
