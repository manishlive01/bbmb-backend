using BBMB.BudgetService.Features.Estimates.Update.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.Update
{
    public class UpdateEstimateValidator : AbstractValidator<UpdateEstimateDto>
    {
        private readonly BudgetContext _context;

        public UpdateEstimateValidator(BudgetContext context)
        {
            _context = context;

            RuleFor(x => x.EstimateDesc)
                .MaximumLength(600)
                .When(x => x.EstimateDesc != null)
                .WithMessage("EstimateDesc must not exceed 600 characters");

            RuleFor(x => x.EstimateNarr)
                .MaximumLength(600)
                .When(x => x.EstimateNarr != null)
                .WithMessage("EstimateNarr must not exceed 600 characters");

            RuleFor(x => x.FinYearEnd)
                .NotEmpty()
                .WithMessage("FinYearEnd is required");

            RuleFor(x => x.DivisionCode)
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
