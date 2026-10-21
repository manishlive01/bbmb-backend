using FluentValidation;

namespace BBMB.BudgetService.Features.Works.SetLegacyCode
{
    /// <summary>
    /// Design §8.6: a legacy code is held as keyed, exactly seven digits (BUD-306, DDD §4.6). A leading zero
    /// survives because the code is text. Everything else (work has no code, first cycle, write-once) is a
    /// stored-state rule and lives in the handler, not here.
    /// </summary>
    public sealed class SetLegacyCodeValidator : AbstractValidator<SetLegacyCodeCommand>
    {
        public SetLegacyCodeValidator()
        {
            RuleFor(x => x.dto.LegacyCode)
                .NotEmpty().WithMessage("Legacy code is required.")
                .Matches(@"^\d{7}$").WithMessage("Legacy code must be exactly seven digits.");
        }
    }
}
