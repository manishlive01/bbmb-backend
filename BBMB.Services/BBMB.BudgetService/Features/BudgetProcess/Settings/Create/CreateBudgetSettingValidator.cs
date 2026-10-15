using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Shared.Validators;
using FluentValidation;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Create
{
    /// <summary>
    /// Design §8.2: key in the six T-17 keys; numericValue required except UNFORESEEN_ACCOUNT_CODE (which takes
    /// accountCodeId and no number, T-17 CHECKs); percentages 0–1000; the rupee limit whole rupees;
    /// UPLOAD_MAX_LINES a positive whole number; effectiveFrom not before now (a limit already applied is not
    /// back-dated, BUD-242); authorityRef required.
    /// </summary>
    public sealed class CreateBudgetSettingValidator : AbstractValidator<CreateBudgetSettingCommand>
    {
        public static readonly IReadOnlyList<string> Keys =
        [
            SettingKey.TsCeilingPct, SettingKey.NoAaLimitRupees, SettingKey.AaDemandCeilingPct,
            SettingKey.AmrIncreasePct, SettingKey.UploadMaxLines, SettingKey.UnforeseenAccountCode,
        ];
        private static readonly string[] PercentKeys = [SettingKey.TsCeilingPct, SettingKey.AaDemandCeilingPct, SettingKey.AmrIncreasePct];

        public CreateBudgetSettingValidator(TimeProvider? clock = null)
        {
            var time = clock ?? TimeProvider.System;
            RuleFor(x => x.Dto.SettingKey).Must(k => Keys.Contains(k))
                .WithMessage($"settingKey must be one of {string.Join(", ", Keys)}.");

            When(x => x.Dto.SettingKey == SettingKey.UnforeseenAccountCode, () =>
            {
                RuleFor(x => x.Dto.NumericValue).Null().WithMessage("numericValue does not apply to UNFORESEEN_ACCOUNT_CODE.");
                RuleFor(x => x.Dto.AccountCodeId).GreaterThan(0).When(x => x.Dto.AccountCodeId.HasValue);
            }).Otherwise(() =>
            {
                RuleFor(x => x.Dto.NumericValue).NotNull().WithMessage("numericValue is required.");
                RuleFor(x => x.Dto.AccountCodeId).Null().WithMessage("accountCodeId applies to UNFORESEEN_ACCOUNT_CODE only.");
            });
            RuleFor(x => x.Dto.NumericValue).InclusiveBetween(0, 1000)
                .When(x => PercentKeys.Contains(x.Dto.SettingKey)).WithMessage("A percentage must be between 0 and 1000.");
            RuleFor(x => x.Dto.NumericValue).WholeRupees().GreaterThanOrEqualTo(0)
                .When(x => x.Dto.SettingKey == SettingKey.NoAaLimitRupees);
            RuleFor(x => x.Dto.NumericValue).Must(v => v is null || (v > 0 && v == decimal.Truncate(v.Value) && v <= int.MaxValue))
                .When(x => x.Dto.SettingKey == SettingKey.UploadMaxLines).WithMessage("UPLOAD_MAX_LINES must be a positive whole number.");
            RuleFor(x => x.Dto.EffectiveFrom).Must(e => e >= time.GetUtcNow())
                .WithMessage("effectiveFrom cannot be in the past.");
            RuleFor(x => x.Dto.AuthorityRef).NotEmpty().MaximumLength(100);
        }
    }
}
