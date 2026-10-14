using BBMB.BudgetService.Features.Estimates.Update.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.Update
{
    // D-3 (design §3): typed to the command so ValidationBehavior resolves and runs it. Typed to
    // UpdateEstimateDto, it never ran.
    public class UpdateEstimateValidator : AbstractValidator<UpdateEstimateCommand>
    {
        private readonly BudgetContext _context;

        public UpdateEstimateValidator(BudgetContext context)
        {
            _context = context;

            RuleFor(x => x.Dto.EstimateDesc)
                .MaximumLength(600)
                .When(x => x.Dto.EstimateDesc != null)
                .WithMessage("EstimateDesc must not exceed 600 characters");

            RuleFor(x => x.Dto.EstimateNarr)
                .MaximumLength(600)
                .When(x => x.Dto.EstimateNarr != null)
                .WithMessage("EstimateNarr must not exceed 600 characters");

            // BUD-013, design §4.5 step 4.
            RuleFor(x => x.Dto.EstimateAmt).WholeRupees();
            RuleFor(x => x.Dto.FinYearEnd)
                .NotEmpty()
                .WithMessage("FinYearEnd is required");

            RuleFor(x => x.Dto.DivisionCode)
                .NotEmpty()
                .WithMessage("DivisionCode is required")
                .MustAsync(DivisionExists)
                .WithMessage("Division with code {PropertyValue} does not exist");
        }

        private async Task<bool> DivisionExists(int divisionCode, CancellationToken cancellationToken)
        {
            return await _context.Divisions.AnyAsync(d => d.DivisionCode == divisionCode, cancellationToken);
        }
    }
}
