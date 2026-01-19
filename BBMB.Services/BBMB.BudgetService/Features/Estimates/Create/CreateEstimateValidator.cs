using BBMB.BudgetService.Features.Estimates.Create.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.Create
{
    public class CreateEstimateValidator : AbstractValidator<CreateEstimateDto>
    {
        private readonly BudgetContext _context;

        public CreateEstimateValidator(BudgetContext context)
        {
            _context = context;

            RuleFor(x => x.WorkCode)
                .NotEmpty().WithMessage("Work code is required")
                .MaximumLength(8).WithMessage("Work code must not exceed 8 characters")
                .MustAsync(async (code, ct) => await _context.Works.AnyAsync(w => w.WorkCode == code, ct))
                .WithMessage("Work code does not exist");

            RuleFor(x => x.EstimateCode)
                .NotEmpty().WithMessage("Estimate code is required")
                .MaximumLength(10).WithMessage("Estimate code must not exceed 10 characters");

            RuleFor(x => x.EstimateDesc)
                .MaximumLength(600).WithMessage("Estimate description must not exceed 600 characters")
                .When(x => !string.IsNullOrEmpty(x.EstimateDesc));

            RuleFor(x => x.EstimateNarr)
                .MaximumLength(600).WithMessage("Estimate narrative must not exceed 600 characters")
                .When(x => !string.IsNullOrEmpty(x.EstimateNarr));

            RuleFor(x => x.EstimateAmt)
                .PrecisionScale(12, 2, true).WithMessage("Estimate amount must have precision (12,2)")
                .When(x => x.EstimateAmt.HasValue);

            RuleFor(x => x.FinYearEnd)
                .NotEmpty().WithMessage("Financial year end is required");

            RuleFor(x => x.DivisionCode)
                .NotEmpty().WithMessage("Division code is required")
                .MustAsync(async (code, ct) => await _context.Divisions.AnyAsync(d => d.DivisionCode == code, ct))
                .WithMessage("Division code does not exist");
        }
    }
}
