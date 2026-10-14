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

            // Mirrors the DDD T-08 CHECKs added by BudgetP1ReferenceStateAndAudit, so a request that the
            // database would refuse gets a 400 naming the field instead of a 500.
            RuleFor(x => x.dto.FinYearClosing)
                .InclusiveBetween(2000, 2099)
                .WithMessage("Financial year closing must be between 2000 and 2099");
            RuleFor(x => x.dto.StartDate)
                .Must((cmd, d) => DateOnly.FromDateTime(d) == new DateOnly(cmd.dto.FinYearClosing - 1, 4, 1))
                .When(x => x.dto.FinYearClosing is >= 2000 and <= 2099)
                .WithMessage("Start date must be 1 April of the year before the closing year");
            RuleFor(x => x.dto.EndDate)
                .Must((cmd, d) => DateOnly.FromDateTime(d) == new DateOnly(cmd.dto.FinYearClosing, 3, 31))
                .When(x => x.dto.FinYearClosing is >= 2000 and <= 2099)
                .WithMessage("End date must be 31 March of the closing year");
            RuleFor(x => x.dto.Closed)
                .Must((cmd, closedDate) =>
                    closedDate == null || closedDate >= cmd.dto.EndDate)
                .WithMessage("Closed date cannot be earlier than end date");
        }
    }
}
