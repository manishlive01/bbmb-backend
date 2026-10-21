using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy
{
    /// <summary>
    /// Design §8.2 (implementation-time change): each ceWings entry names an active CE once, with a wing that
    /// exists in <c>irr_pwr</c> (the T-13 wing_code FK). A failure refuses the whole load (400).
    /// </summary>
    public sealed class LoadBudgetOfficesValidator : AbstractValidator<LoadBudgetOfficesCommand>
    {
        public LoadBudgetOfficesValidator(BudgetContext context)
        {
            RuleFor(x => x.Dto).NotNull();
            RuleFor(x => x.Dto.CeWings)
                .Must(w => w.Select(e => e.CeId).Distinct().Count() == w.Count)
                .WithMessage("Each CE may appear once in ceWings.")
                .When(x => x.Dto is not null);
            RuleForEach(x => x.Dto.CeWings).ChildRules(e =>
            {
                e.RuleFor(w => w.CeId)
                    .GreaterThan(0).WithMessage("ceId must be positive.")
                    .MustAsync((id, ct) => context.CEManagements.AsNoTracking().AnyAsync(c => c.Id == id && c.Status, ct))
                    .WithMessage("ceId is not an active CE.");
                e.RuleFor(w => w.WingCode)
                    .NotEmpty().WithMessage("wingCode is required.")
                    .MaximumLength(2).WithMessage("wingCode must not exceed 2 characters.")
                    .MustAsync((code, ct) => context.PowerIrrigations.AsNoTracking().AnyAsync(p => p.IrrPwrCode == code, ct))
                    .WithMessage("wingCode is not a wing (irr_pwr).");
            }).When(x => x.Dto is not null);
        }
    }
}
