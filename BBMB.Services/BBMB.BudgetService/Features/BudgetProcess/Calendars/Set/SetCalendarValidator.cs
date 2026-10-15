using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Calendars.Set
{
    /// <summary>
    /// Design §8.5 Set: at least one entry; each office level a T-13 level save IT_CELL (BUD-213); no level
    /// repeated in the one request (the upsert key is one row per level). The last date itself carries no
    /// shape rule — a date in the past is permitted and simply warns later (BUD-212).
    /// </summary>
    public sealed class SetCalendarValidator : AbstractValidator<SetCalendarCommand>
    {
        /// <summary>The office levels a calendar date may be set for: T-13 levels save IT_CELL (DDD T-20).</summary>
        public static readonly IReadOnlyList<string> AllowedLevels =
        [
            OfficeLevel.Division, OfficeLevel.Circle, OfficeLevel.Ce, OfficeLevel.Secretariat, OfficeLevel.Finance,
        ];

        public SetCalendarValidator()
        {
            RuleFor(x => x.Dto).NotNull();
            When(x => x.Dto is not null, () =>
            {
                RuleFor(x => x.Dto.Entries).NotEmpty().WithMessage("At least one calendar entry is required.");
                RuleForEach(x => x.Dto.Entries).ChildRules(e =>
                    e.RuleFor(y => y.OfficeLevel).Must(l => AllowedLevels.Contains(l))
                        .WithMessage("officeLevel must be DIVISION, CIRCLE, CE, SECRETARIAT or FINANCE (not IT_CELL)."));
                RuleFor(x => x.Dto.Entries)
                    .Must(entries => entries.Select(e => e.OfficeLevel).Distinct().Count() == entries.Count)
                    .WithMessage("An office level may appear only once in the calendar.");
            });
        }
    }
}
