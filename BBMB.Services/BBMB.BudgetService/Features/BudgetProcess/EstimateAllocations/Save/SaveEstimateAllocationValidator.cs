using BBMB.BudgetService.Shared.Validators;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.Save
{
    /// <summary>Design §8.6 F-29 validator (shape): each amount ≥ 0 and whole rupees; a sanction id positive.</summary>
    public sealed class SaveEstimateAllocationValidator : AbstractValidator<SaveEstimateAllocationCommand>
    {
        public SaveEstimateAllocationValidator()
        {
            RuleForEach(x => x.Dto.Lines).ChildRules(line =>
            {
                line.RuleFor(l => l.TechnicalSanctionId).GreaterThan(0);
                line.RuleFor(l => l.Amount).GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");
                line.RuleFor(l => l.Amount).WholeRupees();
            });
        }
    }
}
