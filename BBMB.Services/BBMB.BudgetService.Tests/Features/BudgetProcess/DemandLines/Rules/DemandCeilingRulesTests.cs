using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Submit;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;
namespace BBMB.BudgetService.Tests.Features.BudgetProcess.DemandLines.Rules;
/// <summary>
/// Design §6.4 P2 rows (P2-12): VR-03 (M-08), VR-04 (M-09), VR-05 (M-10 / BUD-276 justification), BUD-089 absolute values,
/// Save warns / Submit refuses / Finance skips (OI-24, BUD-126), VR-06 (M-13), VR-10 line attachments (M-24). The seeded
/// ceilings are used: NO_AA_LIMIT_RUPEES 10 000, AA_DEMAND_CEILING_PCT 110, AMR_INCREASE_PCT 5.
/// </summary>
public sealed class DemandCeilingRulesTests
{
    private static readonly DemandCeilings Seeded = new(10_000, 110, 5);

    private static DemandLineRuleInput NoAa(decimal? rbe, decimal? obe = 0, long? certificate = null, string? justification = null) => new()
    {
        DemandLineId = 1, SerialNo = 1, LineType = "WORK", WorkId = 1, AccountCodeId = 9, WorkAccountCodeId = 9, WorkClass = "AA",
        WorkHasApproval = false, RbeAmount = rbe, ObeNextAmount = obe, CeCertificateAttachmentId = certificate, JustificationText = justification,
    };

    private static DemandLineRuleInput WithAa(decimal? rbe, decimal? obe, decimal aa = 1000) => new()
    {
        DemandLineId = 1, SerialNo = 1, LineType = "WORK", WorkId = 1, AccountCodeId = 9, WorkAccountCodeId = 9, WorkClass = "AA",
        WorkHasApproval = true, AaAmount = aa, PercentageBase = aa, RbeAmount = rbe, ObeNextAmount = obe,
    };

    private static DemandLineRuleInput Amr(decimal? rbe, decimal? obe, decimal? budget = 1000, int? predecessor = 7,
        string? justification = null, long? ceApproval = null) => new()
    {
        DemandLineId = 1, SerialNo = 1, LineType = "WORK", WorkId = 1, AccountCodeId = 9, WorkAccountCodeId = 9, WorkClass = "AMR",
        PrecedingAmrWorkId = predecessor, AmrApprovedBudget = budget, RbeAmount = rbe, ObeNextAmount = obe,
        JustificationText = justification, CeAmrApprovalAttachmentId = ceApproval,
    };

    private static DemandLineRuleResult Run(RuleMoment moment, DemandLineRuleInput line, string category = "WORKS") =>
        DemandLineRuleEvaluator.Evaluate(new DemandLineRuleContext(category, moment, Seeded), [line]);

    private static IEnumerable<string> Ids(IEnumerable<RuleFailureItemDto> items) => items.Select(i => i.MessageId);

    // ------------------------------------------------------------------ VR-03

    [Fact]
    public void VR03_exactly_10000_is_accepted_and_10001_is_refused_on_Submit_and_warned_on_Save()
    {
        Run(RuleMoment.Submit, NoAa(10_000, 10_000)).Failures.Should().BeEmpty("the limit applies to each column, not their sum");
        var submit = Run(RuleMoment.Submit, NoAa(10_001));
        submit.Failures.Should().ContainSingle(f => f.MessageId == "M-08" && f.Field == "rbeAmount");
        var save = Run(RuleMoment.Save, NoAa(10_001));
        save.Failures.Should().BeEmpty();
        save.Warnings.Should().ContainSingle(w => w.MessageId == "M-08");
    }

    [Fact]
    public void VR03_with_the_CE_certificate_the_figure_is_accepted_but_the_justification_is_mandatory_at_Submit()
    {
        Ids(Run(RuleMoment.Submit, NoAa(50_000, certificate: 3)).Failures).Should().Equal("M-21");
        Run(RuleMoment.Submit, NoAa(50_000, certificate: 3, justification: "Urgent: flood damage")).Failures.Should().BeEmpty();
    }

    [Fact]
    public void BUD089_a_negative_figure_is_measured_by_its_absolute_value()
    {
        Ids(Run(RuleMoment.Submit, NoAa(-10_001)).Failures).Should().Equal("M-08");
        Run(RuleMoment.Submit, NoAa(-10_000)).Failures.Should().BeEmpty();
        Ids(Run(RuleMoment.Submit, WithAa(-600, 501)).Failures).Should().Equal("M-09");
        Ids(Run(RuleMoment.Submit, Amr(-1051, 0)).Failures).Should().Equal("M-10");
    }

    // ------------------------------------------------------------------ VR-04

    [Fact]
    public void VR04_exactly_110_percent_is_accepted_and_one_rupee_more_is_refused_on_Submit_and_warned_on_Save()
    {
        Run(RuleMoment.Submit, WithAa(600, 500)).Failures.Should().BeEmpty("1100 = 110% of 1000");
        Ids(Run(RuleMoment.Submit, WithAa(600, 501)).Failures).Should().Equal("M-09");
        var save = Run(RuleMoment.Save, WithAa(600, 501));
        save.Failures.Should().BeEmpty();
        Ids(save.Warnings).Should().Equal("M-09");
        Run(RuleMoment.Submit, WithAa(50_000, 0)).Failures.Should().NotContain(f => f.MessageId == "M-08", "VR-03 is for works without an approval");
    }

    // ------------------------------------------------------------------ VR-05

    [Fact]
    public void VR05_exactly_5_percent_above_is_accepted_and_one_rupee_more_is_refused_on_Submit()
    {
        Run(RuleMoment.Submit, Amr(1050, 1050)).Failures.Should().BeEmpty();
        var r = Run(RuleMoment.Submit, Amr(1051, 1050));
        r.Failures.Should().ContainSingle(f => f.MessageId == "M-10" && f.Field == "rbeAmount");
        Ids(Run(RuleMoment.Save, Amr(1051, 1050)).Warnings).Should().Equal("M-10");
    }

    [Fact]
    public void VR05_crossed_is_accepted_with_both_the_justification_and_the_CE_AMR_approval()
    {
        Ids(Run(RuleMoment.Submit, Amr(2000, 0, justification: "Breach", ceApproval: null)).Failures).Should().Equal("M-10");
        Ids(Run(RuleMoment.Submit, Amr(2000, 0, justification: null, ceApproval: 4)).Failures).Should().Equal("M-10");
        Run(RuleMoment.Submit, Amr(2000, 0, justification: "Breach", ceApproval: 4)).Failures.Should().BeEmpty();
    }

    [Fact]
    public void BUD276_with_no_approved_budget_or_no_column_14_VR05_is_not_applied_and_the_justification_is_mandatory()
    {
        Ids(Run(RuleMoment.Submit, Amr(99_999, 0, budget: null)).Failures).Should().Equal("M-21");
        Ids(Run(RuleMoment.Submit, Amr(99_999, 0, budget: 1000, predecessor: null)).Failures).Should().Equal("M-21");
        Run(RuleMoment.Submit, Amr(99_999, 0, budget: null, justification: "Continues no work")).Failures.Should().BeEmpty();
        Run(RuleMoment.Save, Amr(99_999, 0, budget: null)).Failures.Should().BeEmpty("the justification is required at Submit");
    }

    // ------------------------------------------------------------------ moments

    [Theory]
    [InlineData(RuleMoment.FinanceAlter)]
    [InlineData(RuleMoment.FinanceFinalise)]
    public void BUD126_Finance_moments_skip_the_three_ceilings(RuleMoment moment)
    {
        var all = new[] { NoAa(50_000), WithAa(5000, 5000), Amr(99_999, 0), Amr(2000, 2000) };
        foreach (var line in all)
        {
            var r = Run(moment, line);
            r.Failures.Concat(r.Warnings).Should().NotContain(f => f.MessageId == "M-08" || f.MessageId == "M-09" || f.MessageId == "M-10");
        }
    }

    [Fact]
    public void Reviewer_Forward_refuses_and_reviewer_alteration_warns()
    {
        Ids(Run(RuleMoment.ReviewerForward, NoAa(10_001)).Failures).Should().Equal("M-08");
        Ids(Run(RuleMoment.ReviewerAlter, NoAa(10_001)).Warnings).Should().Equal("M-08");
    }

    [Fact]
    public void A_null_figure_is_a_part_entry_and_is_not_measured()
    {
        Run(RuleMoment.Save, NoAa(null, null)).Warnings.Should().BeEmpty();
        Run(RuleMoment.Save, WithAa(null, 1100)).Warnings.Should().BeEmpty();
    }

    // ------------------------------------------------------------------ VR-06, VR-10

    [Fact]
    public void VR06_head_not_assigned_is_M13_at_every_moment()
    {
        var head = new DemandLineRuleInput { DemandLineId = 2, SerialNo = 2, LineType = "HEAD", AccountCodeId = 9, HeadAssigned = false, RbeAmount = 1, ObeNextAmount = 1 };
        foreach (var m in Enum.GetValues<RuleMoment>())
            Ids(DemandLineRuleEvaluator.Evaluate(new DemandLineRuleContext("DNA", m, Seeded), [head]).Failures).Should().Contain("M-13", m.ToString());
    }

    [Fact]
    public void VR10_a_CE_attachment_not_of_evidence_class_is_refused()
    {
        var line = NoAa(100) with { CeCertificateAttachmentId = 5, CeCertificateIsEvidence = false };
        Run(RuleMoment.Save, line).Failures.Should().ContainSingle(f => f.MessageId == "M-24" && f.Field == "ceCertificateAttachmentId");
    }

    [Fact]
    public void Without_ceilings_in_the_context_none_is_applied_as_in_P1()
    {
        DemandLineRuleEvaluator.Evaluate(new DemandLineRuleContext("WORKS", RuleMoment.Submit), [NoAa(99_999)]).Failures.Should().BeEmpty();
    }
}

/// <summary>Integration: VR-06 at Submit lists the line (P2-12 acceptance).</summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class DemandCeilingRulesSubmitTests(BudgetDatabaseFixture fixture)
{
    [Fact]
    public async Task VR06_Submit_with_an_unassigned_head_lists_M13_for_that_line()
    {
        var w = await SpineWorld.CreateAsync(fixture, $"vr06_{Guid.NewGuid():N}"[..21]);
        // A head-wise line carried from before DB-02 (P1 data): seeded with triggers suspended, test data only. The
        // world's only assignment to Division A is then removed, which DB-02 itself would refuse while the line is live.
        var (pc, ptx) = await w.BeginAsync(w.ItCell, "OPEN_PROPOSALS");
        await using (pc)
        {
            await using var p = new NpgsqlCommand(
                "INSERT INTO budget.proposal (cycle_id, category, origin_office_id, state_code, holder_office_id) " +
                $"VALUES ({w.CycleId}, 'DNA', {w.DivisionAOfficeId}, 'S-01', {w.DivisionAOfficeId})", pc, ptx);
            await p.ExecuteNonQueryAsync();
            await ptx.CommitAsync();
        }
        var a = w.ItCell.Actor!;
        await using (var c = new NpgsqlConnection(w.ConnectionString))
        {
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SET session_replication_role = replica; " +
                "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, account_code_id, rbe_amount, obe_next_amount, " +
                "created_at, created_by, created_charge_id) " +
                $"SELECT proposal_id, 'HEAD', 1, {w.AccountCodeId}, 10, 10, now(), '{a.EmployeeId}', '{a.ChargeId}' FROM budget.proposal WHERE category = 'DNA'; " +
                $"UPDATE budget.account_head_assignment SET removed_at = now(), removed_by = '{w.ItCell.Actor!.EmployeeId}' WHERE office_id = {w.DivisionAOfficeId}", c);
            await cmd.ExecuteNonQueryAsync();
        }
        long proposalId;
        await using (var ctx = w.NewContext())
            proposalId = ctx.BudgetProposals.Single(p => p.Category == "DNA").ProposalId;
        await using var hctx = w.NewContext();
        var r = await new SubmitProposalHandler(hctx, w.DivisionA, TestServices.Transitions(hctx, w.DivisionA),
                new DemandLineRules(new DemandLineRuleInputsLoader(hctx)))
            .Handle(new SubmitProposalCommand(proposalId), default);
        r.IsSucced.Should().BeFalse();
        ((Response<RuleFailuresDto>)r).Data!.Failures.Should().ContainSingle(f => f.MessageId == "M-13" && f.SerialNo == 1 && f.Field == "accountCodeId");
    }
}
