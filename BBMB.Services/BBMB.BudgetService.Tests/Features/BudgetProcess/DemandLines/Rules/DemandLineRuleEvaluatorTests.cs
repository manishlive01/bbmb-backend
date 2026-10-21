using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.DemandLines.Rules;

/// <summary>Design §6.4 P1 rows (VR-09, VR-14, VR-15, completeness, BUD-243, §5.5 row i).</summary>
public sealed class DemandLineRuleEvaluatorTests
{
    private static DemandLineRuleInput Work(decimal? rbe = 100, decimal? obe = 50, decimal? spent = null, int workId = 1, int serial = 1) => new()
    {
        DemandLineId = serial, SerialNo = serial, LineType = "WORK", WorkId = workId, AccountCodeId = 9, WorkAccountCodeId = 9,
        RbeAmount = rbe, ObeNextAmount = obe, ExpenditureUpToDate = spent,
    };

    private static DemandLineRuleResult Run(RuleMoment moment, params DemandLineRuleInput[] lines) =>
        DemandLineRuleEvaluator.Evaluate(new DemandLineRuleContext("WORKS", moment), lines);

    [Fact]
    public void VR15_SaveAtExpenditureMinusOneRefusedM36()
    {
        var r = Run(RuleMoment.Save, Work(rbe: 999, spent: 1000));
        r.Failures.Should().ContainSingle().Which.Should().Match<BBMB.BudgetService.Shared.ResponseDTOs.RuleFailureItemDto>(
            f => f.MessageId == "M-36" && f.Field == "rbeAmount" && f.SerialNo == 1);
    }

    [Fact]
    public void VR15_SaveAtExpenditureAccepted()
    {
        var r = Run(RuleMoment.Save, Work(rbe: 1000, spent: 1000));
        r.Refused.Should().BeFalse();
        r.Flags.Single().FloorCheckUnchecked.Should().BeFalse();
    }

    [Fact]
    public void VR15_ExpenditureUnavailableAcceptedAndFlagged()
    {
        var r = Run(RuleMoment.Submit, Work(rbe: 1, spent: null));
        r.Refused.Should().BeFalse();
        r.Flags.Single().FloorCheckUnchecked.Should().BeTrue();
    }

    [Fact]
    public void VR15_skipped_when_rbe_is_null_on_Save()
    {
        var r = Run(RuleMoment.Save, Work(rbe: null, spent: 1000));
        r.Refused.Should().BeFalse("a part-made entry is not checked (BUD-241)");
        r.Flags.Single().FloorCheckUnchecked.Should().BeFalse();
    }

    [Theory]
    [InlineData(RuleMoment.ReviewerAlter)]
    [InlineData(RuleMoment.FinanceAlter)]
    [InlineData(RuleMoment.FinanceFinalise)]
    public void BUD317_VR15_floor_is_not_lifted_at_any_level(RuleMoment moment)
    {
        Run(moment, Work(rbe: 10, spent: 11)).Failures.Should().Contain(f => f.MessageId == "M-36");
    }

    [Fact]
    public void BUD318_VR15_is_not_applied_to_RECEIPTS()
    {
        var head = new DemandLineRuleInput { DemandLineId = 1, SerialNo = 1, LineType = "HEAD", AccountCodeId = 9, RbeAmount = 1, ObeNextAmount = 1, ExpenditureUpToDate = 50 };
        DemandLineRuleEvaluator.Evaluate(new DemandLineRuleContext("RECEIPTS", RuleMoment.Submit), [head]).Refused.Should().BeFalse();
    }

    [Fact]
    public void BUD010_completeness_refuses_on_Submit_but_not_on_Save()
    {
        Run(RuleMoment.Save, Work(rbe: null, obe: null)).Refused.Should().BeFalse();
        var submit = Run(RuleMoment.Submit, Work(rbe: null, obe: null));
        submit.Failures.Select(f => (f.Field, f.MessageId)).Should().BeEquivalentTo(new[] { ("rbeAmount", "M-21"), ("obeNextAmount", "M-21") });
        var unforeseen = new DemandLineRuleInput { LineType = "UNFORESEEN", RbeAmount = 1, ObeNextAmount = 1, UnforeseenName = " " };
        Run(RuleMoment.Upload, unforeseen).Failures.Should().ContainSingle(f => f.Field == "unforeseenName");
    }

    [Fact]
    public void BUD243_nil_or_negative_base_sets_the_flag_with_an_M32_warning()
    {
        var r = Run(RuleMoment.Save, Work() with { PercentageBase = 0 }, Work(workId: 2, serial: 2) with { PercentageBase = -5 },
            Work(workId: 3, serial: 3) with { PercentageBase = 100 });
        r.Flags.Select(f => f.PctCheckNotApplied).Should().Equal(true, true, false);
        r.Warnings.Should().HaveCount(2).And.OnlyContain(w => w.MessageId == "M-32");
        r.Refused.Should().BeFalse();
    }

    [Fact]
    public void VR14_duplicate_head_line_gets_M35_and_duplicate_work_M25()
    {
        var head1 = new DemandLineRuleInput { DemandLineId = 1, SerialNo = 1, LineType = "HEAD", AccountCodeId = 9 };
        var head2 = head1 with { DemandLineId = null, SerialNo = null };
        DemandLineRuleEvaluator.Evaluate(new DemandLineRuleContext("DNA", RuleMoment.Save), [head1, head2]).Failures
            .Should().HaveCount(2).And.OnlyContain(f => f.MessageId == "M-35");
        Run(RuleMoment.Save, Work(serial: 1), Work(serial: 2)).Failures.Should().HaveCount(2).And.OnlyContain(f => f.MessageId == "M-25");
    }

    [Fact]
    public void VR09_unusable_code_or_missing_work_gets_M05_and_pre_ddd_work_M21()
    {
        Run(RuleMoment.Save, Work() with { AccountCodeUsable = false }).Failures.Should().ContainSingle(f => f.MessageId == "M-05" && f.Field == "accountCodeId");
        Run(RuleMoment.Save, Work() with { WorkExists = false }).Failures.Should().ContainSingle(f => f.MessageId == "M-05" && f.Field == "workId");
        Run(RuleMoment.Save, Work() with { WorkPreDdd = true }).Failures.Should().ContainSingle(f => f.MessageId == "M-21" && f.Field == "workId");
    }

    [Fact]
    public void S55i_WORK_line_code_must_equal_the_work_resolved_code()
    {
        Run(RuleMoment.Save, Work() with { WorkAccountCodeId = 10 }).Failures.Should().ContainSingle(f => f.MessageId == "M-22" && f.Field == "accountCodeId");
    }

    [Fact]
    public void BUD122_justification_required_on_Submit_where_an_observation_exists()
    {
        Run(RuleMoment.Save, Work() with { HasObservation = true }).Refused.Should().BeFalse();
        Run(RuleMoment.Submit, Work() with { HasObservation = true }).Failures.Should().ContainSingle(f => f.Field == "justificationText");
        Run(RuleMoment.Submit, Work() with { HasObservation = true, JustificationText = "Reasons" }).Refused.Should().BeFalse();
        Run(RuleMoment.FinanceFinalise, Work() with { HasObservation = true }).Refused.Should().BeFalse();
    }
}

/// <summary>The loader against a migrated database (resolved work code, no expenditure → unavailable).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class DemandLineRuleInputsLoaderTests(BudgetDatabaseFixture fixture)
{
    [Fact]
    public async Task S64_loader_resolves_the_work_code_and_reports_expenditure_unavailable()
    {
        var w = await SpineWorld.CreateAsync(fixture, $"dlr_{Guid.NewGuid():N}"[..20]);
        await using var ctx = w.NewContext();
        var proposal = await ctx.BudgetProposals.AsNoTracking().SingleAsync(p => p.ProposalId == w.WorksProposalAId);
        var candidate = new DemandLine { ProposalId = proposal.ProposalId, LineType = "WORK", WorkId = w.WorkAId, AccountCodeId = w.AccountCodeId, RbeAmount = 5 };

        var r = await new DemandLineRules(new DemandLineRuleInputsLoader(ctx)).EvaluateAsync(proposal, [candidate], RuleMoment.Save, "DIVISION");

        r.Refused.Should().BeFalse(string.Join("; ", r.Failures.Select(f => f.Message)));
        r.Flags.Single().FloorCheckUnchecked.Should().BeTrue("no expenditure is keyed");
        var inputs = await new DemandLineRuleInputsLoader(ctx).LoadAsync(proposal, [candidate], default);
        inputs.Single().WorkAccountCodeId.Should().Be(w.AccountCodeId);
    }
}
