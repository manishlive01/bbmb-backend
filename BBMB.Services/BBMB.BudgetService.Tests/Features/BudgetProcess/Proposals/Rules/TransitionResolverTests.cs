using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using FluentAssertions;

namespace BBMB.BudgetService.Tests.Features.BudgetProcess.Proposals.Rules;

/// <summary>Design §6.2: target resolution for every row, and the recall rule (BUD-214, BUD-215).</summary>
public sealed class TransitionResolverTests
{
    // Office ids: 1 Finance, 2 Secretariat, 3 CE, 4 Circle, 5 Division.
    private static readonly TransitionOffices Offices = new(1, 2, new Dictionary<long, string>
    {
        [1] = "FINANCE", [2] = "SECRETARIAT", [3] = "CE", [4] = "CIRCLE", [5] = "DIVISION",
    });

    private static readonly Dictionary<long, long?> Parent = new() { [1] = null, [2] = 1, [3] = 2, [4] = 3, [5] = 4 };

    private static TransitionProposal P(string state, long origin, long holder, string category = "WORKS") =>
        new(state, category, origin, Offices.LevelById[origin], Parent[origin], holder, Parent[holder]);

    private static TransitionHistoryRow H(string from, string action, string to, long fromOffice, long toOffice) =>
        new(from, action, to, fromOffice, toOffice);

    private static ResolvedTransition? R(string action, TransitionProposal p, long actor, params TransitionHistoryRow[] history) =>
        TransitionResolver.Resolve(action, p, history, actor, Offices);

    public static TheoryData<string, string, long, long, long, string, long> Rows => new()
    {
        // row id, action, origin, holder (= actor), -> to-state, to-office
        { "T01", "SUBMIT", 5, 5, 5, "S-02", 4 },
        { "T02", "SUBMIT", 4, 4, 4, "S-05", 3 },
        { "T03", "SUBMIT", 3, 3, 3, "S-07", 2 },
    };

    [Theory]
    [MemberData(nameof(Rows))]
    public void BUD142_submit_target_depends_on_the_framing_office(string row, string action, long origin, long holder, long actor, string to, long toOffice)
    {
        var r = R(action, P("S-01", origin, holder, origin == 5 ? "WORKS" : "DNA"), actor);
        r.Should().Be(new ResolvedTransition("S-01", action, to, toOffice), row);
    }

    [Theory]
    [InlineData(5L, "S-02", 4L)] // T08
    [InlineData(4L, "S-05", 3L)] // T08b
    [InlineData(3L, "S-07", 2L)] // T08c
    public void T08_resubmit_targets(long origin, string to, long toOffice)
    {
        R("RESUBMIT", P("S-04", origin, origin, "DNA"), origin).Should().Be(new ResolvedTransition("S-04", "RESUBMIT", to, toOffice));
    }

    [Theory]
    [InlineData("S-02", "S-03", 4L)] // T05
    [InlineData("S-05", "S-06", 3L)] // T10
    [InlineData("S-07", "S-08", 2L)] // T14
    [InlineData("S-09", "S-10", 1L)] // T18
    public void Open_targets(string from, string to, long holder)
    {
        R("OPEN", P(from, 5, holder), holder).Should().Be(new ResolvedTransition(from, "OPEN", to, holder));
    }

    [Fact]
    public void T06_T11_T15_return_one_level_down_the_channel()
    {
        var submit = H("S-01", "SUBMIT", "S-02", 5, 4);
        var open = H("S-02", "OPEN", "S-03", 4, 4);
        R("RETURN", P("S-03", 5, 4), 4, submit, open).Should().Be(new ResolvedTransition("S-03", "RETURN", "S-04", 5));

        var fwd = H("S-03", "FORWARD", "S-05", 4, 3);
        var open2 = H("S-05", "OPEN", "S-06", 3, 3);
        R("RETURN", P("S-06", 5, 3), 3, submit, open, fwd, open2).Should().Be(new ResolvedTransition("S-06", "RETURN", "S-03", 4));

        var fwd2 = H("S-06", "FORWARD", "S-07", 3, 2);
        var open3 = H("S-07", "OPEN", "S-08", 2, 2);
        R("RETURN", P("S-08", 5, 2), 2, submit, open, fwd, open2, fwd2, open3).Should().Be(new ResolvedTransition("S-08", "RETURN", "S-06", 3));
    }

    [Fact]
    public void T11b_T15b_return_to_a_framing_Circle_or_CE_goes_to_S04()
    {
        var circleSubmit = H("S-01", "SUBMIT", "S-05", 4, 3);
        R("RETURN", P("S-06", 4, 3, "DNA"), 3, circleSubmit, H("S-05", "OPEN", "S-06", 3, 3))
            .Should().Be(new ResolvedTransition("S-06", "RETURN", "S-04", 4));

        var ceSubmit = H("S-01", "SUBMIT", "S-07", 3, 2);
        R("RETURN", P("S-08", 3, 2, "DNA"), 2, ceSubmit, H("S-07", "OPEN", "S-08", 2, 2))
            .Should().Be(new ResolvedTransition("S-08", "RETURN", "S-04", 3));
    }

    [Fact]
    public void T07_T12_T16_T19_T20_T21_forward_send_back_finalise_reopen()
    {
        R("FORWARD", P("S-03", 5, 4), 4).Should().Be(new ResolvedTransition("S-03", "FORWARD", "S-05", 3));
        R("FORWARD", P("S-06", 5, 3), 3).Should().Be(new ResolvedTransition("S-06", "FORWARD", "S-07", 2));
        R("FORWARD_FINANCE", P("S-08", 5, 2), 2).Should().Be(new ResolvedTransition("S-08", "FORWARD_FINANCE", "S-09", 1));
        R("SEND_BACK", P("S-10", 5, 1), 1).Should().Be(new ResolvedTransition("S-10", "SEND_BACK", "S-08", 2));
        R("FINALISE", P("S-10", 5, 1), 1).Should().Be(new ResolvedTransition("S-10", "FINALISE", "S-11", 1));
        R("REOPEN", P("S-11", 5, 1), 1).Should().Be(new ResolvedTransition("S-11", "REOPEN", "S-10", 1));
    }

    [Fact]
    public void BUD214_recall_target_is_the_from_state_of_the_last_movement()
    {
        R("RECALL", P("S-02", 5, 4), 5, H("S-01", "SUBMIT", "S-02", 5, 4))
            .Should().Be(new ResolvedTransition("S-02", "RECALL", "S-01", 5), "T04");
        R("RECALL", P("S-02", 5, 4), 5, H("S-04", "RESUBMIT", "S-02", 5, 4))
            .Should().Be(new ResolvedTransition("S-02", "RECALL", "S-04", 5), "T04b");
        R("RECALL", P("S-05", 5, 3), 4, H("S-03", "FORWARD", "S-05", 4, 3))
            .Should().Be(new ResolvedTransition("S-05", "RECALL", "S-03", 4), "T09");
        R("RECALL", P("S-05", 4, 3, "DNA"), 4, H("S-01", "SUBMIT", "S-05", 4, 3))
            .Should().Be(new ResolvedTransition("S-05", "RECALL", "S-01", 4), "T09b");
        R("RECALL", P("S-05", 4, 3, "DNA"), 4, H("S-04", "RESUBMIT", "S-05", 4, 3))
            .Should().Be(new ResolvedTransition("S-05", "RECALL", "S-04", 4), "T09c");
        R("RECALL", P("S-07", 5, 2), 3, H("S-06", "FORWARD", "S-07", 3, 2))
            .Should().Be(new ResolvedTransition("S-07", "RECALL", "S-06", 3), "T13");
        R("RECALL", P("S-07", 3, 2, "DNA"), 3, H("S-01", "SUBMIT", "S-07", 3, 2))
            .Should().Be(new ResolvedTransition("S-07", "RECALL", "S-01", 3), "T13b");
        R("RECALL", P("S-07", 3, 2, "DNA"), 3, H("S-04", "RESUBMIT", "S-07", 3, 2))
            .Should().Be(new ResolvedTransition("S-07", "RECALL", "S-04", 3), "T13c");
        R("RECALL", P("S-09", 5, 1), 2, H("S-08", "FORWARD_FINANCE", "S-09", 2, 1))
            .Should().Be(new ResolvedTransition("S-09", "RECALL", "S-08", 2), "T17");
    }

    [Fact]
    public void BUD215_recall_refused_after_OPEN_and_by_another_office()
    {
        R("RECALL", P("S-03", 5, 4), 5, H("S-01", "SUBMIT", "S-02", 5, 4), H("S-02", "OPEN", "S-03", 4, 4))
            .Should().BeNull("the office above has opened it");
        R("RECALL", P("S-02", 5, 4), 4, H("S-01", "SUBMIT", "S-02", 5, 4))
            .Should().BeNull("only the office that made the last movement may recall");
        R("RECALL", P("S-01", 5, 5), 5).Should().BeNull("nothing to recall");
    }

    [Fact]
    public void M19_actions_with_no_target_from_the_state_resolve_to_null()
    {
        R("SUBMIT", P("S-03", 5, 4), 4).Should().BeNull();
        R("OPEN", P("S-01", 5, 5), 5).Should().BeNull();
        R("RETURN", P("S-02", 5, 4), 4).Should().BeNull();
        R("FORWARD_FINANCE", P("S-06", 5, 3), 3).Should().BeNull();
    }

    [Theory]
    [InlineData("SUBMIT", "S-01", "WORKS", "F07.Submit")]
    [InlineData("SUBMIT", "S-01", "DNA", "F11.Submit")]
    [InlineData("OPEN", "S-09", "WORKS", "Finance.Open")]
    [InlineData("OPEN", "S-02", "WORKS", "Review.Examine")]
    [InlineData("FORWARD_FINANCE", "S-08", "DNA", "Review.ForwardFinance")]
    [InlineData("SEND_BACK", "S-10", "DNA", "F13.Act")]
    [InlineData("FINALISE", "S-10", "WORKS", "F10.Act")]
    [InlineData("RECALL", "S-02", "WORKS", "Proposal.Recall")]
    public void BUD033_right_per_transition(string action, string state, string category, string right)
    {
        TransitionResolver.RightFor(action, state, category).Should().Be(right);
    }
}
