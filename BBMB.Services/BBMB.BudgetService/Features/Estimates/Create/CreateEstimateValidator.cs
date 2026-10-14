using BBMB.BudgetService.Features.Estimates.Create.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.Create
{
    // D-3 (design §3): typed to the command so ValidationBehavior resolves and runs it. Typed to
    // CreateEstimateDto, it never ran.
    public class CreateEstimateValidator : AbstractValidator<CreateEstimateCommand>
    {
        private readonly BudgetContext _context;

        public CreateEstimateValidator(BudgetContext context)
        {
            _context = context;

            RuleFor(x => x.dto.WorkCode)
                .NotNull().WithMessage("Work code is required")
                .GreaterThan(0).WithMessage("Work code must be valid")
                .MustAsync(async (code, ct) =>
                    await _context.Works.AnyAsync(w => w.WorkCode == code, ct))
                .WithMessage("Work code does not exist");

            RuleFor(x => x.dto.EstimateCode)
                .NotEmpty().WithMessage("Estimate code is required")
                .MaximumLength(10).WithMessage("Estimate code must not exceed 10 characters");

            RuleFor(x => x.dto.EstimateDesc)
                .MaximumLength(600).WithMessage("Estimate description must not exceed 600 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.EstimateDesc));

            RuleFor(x => x.dto.EstimateNarr)
                .MaximumLength(600).WithMessage("Estimate narrative must not exceed 600 characters")
                .When(x => !string.IsNullOrEmpty(x.dto.EstimateNarr));

            // BUD-013, design §4.5 step 4: whole rupees replace PrecisionScale(12,2).
            RuleFor(x => x.dto.EstimateAmt).WholeRupees();

            RuleFor(x => x.dto.FinYearEnd)
                .NotEmpty().WithMessage("Financial year end is required");

            RuleFor(x => x.dto.DivisionCode)
                .NotEmpty().WithMessage("Division code is required")
                .MustAsync(async (code, ct) => await _context.Divisions.AnyAsync(d => d.DivisionCode == code, ct))
                .WithMessage("Division code does not exist");
        }
    }
}
