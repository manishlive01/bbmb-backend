using BBMB.BudgetService.Shared.Validators;
using FluentAssertions;
using FluentValidation;

namespace BBMB.BudgetService.Tests.Shared.Validators;

public sealed class RupeeRuleExtensionsTests
{
    private sealed record Money(decimal Amount, decimal? Optional);

    private sealed class MoneyValidator : AbstractValidator<Money>
    {
        public MoneyValidator()
        {
            RuleFor(x => x.Amount).WholeRupees();
            RuleFor(x => x.Optional).WholeRupees();
        }
    }

    private sealed record Dated(DateOnly Date, DateOnly? Optional);

    private sealed class DatedValidator : AbstractValidator<Dated>
    {
        public DatedValidator(TimeProvider clock)
        {
            RuleFor(x => x.Date).NotAfterTodayIst(clock);
            RuleFor(x => x.Optional).NotAfterTodayIst(clock);
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Theory]
    [InlineData("100", true)]
    [InlineData("100.50", false)]
    [InlineData("-500", true)]
    [InlineData("9999999999999", true)]
    [InlineData("-9999999999999", true)]
    [InlineData("10000000000000", false)]
    [InlineData("0.01", false)]
    [InlineData("0", true)]
    public void BUD013_DV02_whole_rupees(string value, bool valid)
    {
        var result = new MoneyValidator().Validate(new Money(decimal.Parse(value), null));
        result.IsValid.Should().Be(valid);
        if (!valid)
        {
            result.Errors.Should().ContainSingle().Which.ErrorCode.Should().Be("M-22");
            result.Errors[0].ErrorMessage.Should().Contain("Amount").And.Contain("whole rupees");
        }
    }

    [Fact]
    public void BUD013_null_is_skipped_and_paise_on_optional_refused()
    {
        new MoneyValidator().Validate(new Money(1, null)).IsValid.Should().BeTrue();
        new MoneyValidator().Validate(new Money(1, 100.50m)).Errors.Should().ContainSingle()
            .Which.ErrorCode.Should().Be("M-22");
    }

    [Fact]
    public void BUD014_tomorrow_in_IST_is_refused_today_in_IST_accepted()
    {
        // 20:00 UTC on 1 Jan is 01:30 IST on 2 Jan: "today" is 2 Jan in India although UTC still says 1 Jan.
        var clock = new FixedClock(new DateTimeOffset(2026, 1, 1, 20, 0, 0, TimeSpan.Zero));
        var v = new DatedValidator(clock);

        v.Validate(new Dated(new DateOnly(2026, 1, 2), null)).IsValid.Should().BeTrue();
        v.Validate(new Dated(new DateOnly(2026, 1, 3), null)).Errors.Should().ContainSingle()
            .Which.ErrorCode.Should().Be("M-22");
        v.Validate(new Dated(new DateOnly(2026, 1, 2), new DateOnly(2026, 1, 3))).IsValid.Should().BeFalse();
        v.Validate(new Dated(new DateOnly(2025, 12, 31), null)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void BUD014_system_clock_today_in_IST_accepted()
    {
        var today = RupeeRuleExtensions.TodayIst();
        var v = new DatedValidator(TimeProvider.System);
        v.Validate(new Dated(today, today)).IsValid.Should().BeTrue();
        v.Validate(new Dated(today.AddDays(1), null)).IsValid.Should().BeFalse();
    }
}
