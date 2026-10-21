using BBMB.BudgetService.Shared.Messages;
using FluentValidation;

namespace BBMB.BudgetService.Shared.Validators
{
    /// <summary>
    /// Shape rules shared by every slice validator (design §4.2, §4.5, §5.1).
    /// <list type="bullet">
    /// <item><see cref="WholeRupees{T}(IRuleBuilder{T, decimal})"/>: BUD-013, DDD §1.7 domain <c>budget.rupees</c>
    /// (<c>VALUE = trunc(VALUE) AND abs(VALUE) &lt;= 9999999999999</c>). Paise are refused, never rounded (DV-02).
    /// Sign is not checked here; validators add <c>&gt; 0</c> where the figure cannot be negative.</item>
    /// <item><see cref="NotAfterTodayIst{T}(IRuleBuilder{T, DateOnly}, TimeProvider?)"/>: BUD-014, a date later than
    /// today in India Standard Time is refused with M-22 (a value not of the required form, the form stated).
    /// M-21 ("required") is left to <c>NotEmpty</c> rules.</item>
    /// </list>
    /// </summary>
    public static class RupeeRuleExtensions
    {
        public const decimal MaxRupees = 9_999_999_999_999m;

        private const string RupeesForm = "whole rupees, at most 13 digits";
        private const string DateForm = "a date not later than today";

        public static readonly TimeZoneInfo India = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

        public static bool IsWholeRupees(decimal v) => v == decimal.Truncate(v) && Math.Abs(v) <= MaxRupees;

        public static IRuleBuilderOptions<T, decimal> WholeRupees<T>(this IRuleBuilder<T, decimal> rule) =>
            rule.Must(IsWholeRupees)
                .WithErrorCode(BudgetMessages.M22)
                .WithMessage(M22For(RupeesForm));

        public static IRuleBuilderOptions<T, decimal?> WholeRupees<T>(this IRuleBuilder<T, decimal?> rule) =>
            rule.Must(v => v is null || IsWholeRupees(v.Value))
                .WithErrorCode(BudgetMessages.M22)
                .WithMessage(M22For(RupeesForm));

        /// <summary>Today in Asia/Kolkata, from <paramref name="clock"/> (system clock when null).</summary>
        public static DateOnly TodayIst(TimeProvider? clock = null) =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTime((clock ?? TimeProvider.System).GetUtcNow(), India).DateTime);

        public static IRuleBuilderOptions<T, DateOnly> NotAfterTodayIst<T>(
            this IRuleBuilder<T, DateOnly> rule, TimeProvider? clock = null) =>
            rule.Must(d => d <= TodayIst(clock))
                .WithErrorCode(BudgetMessages.M22)
                .WithMessage(M22For(DateForm + " (India Standard Time)"));

        public static IRuleBuilderOptions<T, DateOnly?> NotAfterTodayIst<T>(
            this IRuleBuilder<T, DateOnly?> rule, TimeProvider? clock = null) =>
            rule.Must(d => d is null || d.Value <= TodayIst(clock))
                .WithErrorCode(BudgetMessages.M22)
                .WithMessage(M22For(DateForm + " (India Standard Time)"));

        // "{PropertyName}" is substituted by FluentValidation, so the message names the field (BUD-012).
        private static string M22For(string form) => $"{BudgetMessages.Format(BudgetMessages.M22)} Field: {{PropertyName}}. Expected: {form}.";
    }
}
