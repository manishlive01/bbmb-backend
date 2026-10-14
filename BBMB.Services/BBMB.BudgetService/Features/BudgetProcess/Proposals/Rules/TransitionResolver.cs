using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules
{
    /// <summary>The proposal as the resolver needs it.</summary>
    public sealed record TransitionProposal(
        string StateCode,
        string Category,
        long OriginOfficeId,
        string OriginLevel,
        long? OriginParentOfficeId,
        long HolderOfficeId,
        long? HolderParentOfficeId);

    /// <summary>One T-33 row of the proposal's history (newest last).</summary>
    public sealed record TransitionHistoryRow(string FromState, string ActionCode, string ToState, long FromOfficeId, long ToOfficeId);

    /// <summary>A resolved move: the triple to look up in T-16 and the office the proposal goes to.</summary>
    public sealed record ResolvedTransition(string FromState, string ActionCode, string ToState, long ToOfficeId);

    /// <summary>The office levels the resolver needs to know besides the proposal's own chain.</summary>
    public sealed record TransitionOffices(long? FinanceOfficeId, long? SecretariatOfficeId, IReadOnlyDictionary<long, string> LevelById);

    /// <summary>
    /// Pure resolution of the target state and office of a move (design §6.2 and its rules list). It does not
    /// decide whether the move is permitted: the caller looks the triple up in T-16 (absent → M-19) and checks
    /// holder, rights and remark. Returns null where the action has no target from this state.
    /// </summary>
    public static class TransitionResolver
    {
        /// <summary>SUBMIT / RESUBMIT target by the framing office's level (T01–T03, T08–T08c; BUD-142).</summary>
        public static string? SubmitTarget(string originLevel) => originLevel switch
        {
            OfficeLevel.Division => ProposalStateCode.S02,
            OfficeLevel.Circle => ProposalStateCode.S05,
            OfficeLevel.Ce => ProposalStateCode.S07,
            _ => null,
        };

        /// <summary>The "under examination" state held at a reviewing level (target of a RETURN to it).</summary>
        public static string? ExaminationStateOf(string level) => level switch
        {
            OfficeLevel.Circle => ProposalStateCode.S03,
            OfficeLevel.Ce => ProposalStateCode.S06,
            OfficeLevel.Secretariat => ProposalStateCode.S08,
            _ => null,
        };

        public static ResolvedTransition? Resolve(
            string action,
            TransitionProposal p,
            IReadOnlyList<TransitionHistoryRow> history,
            long actorOfficeId,
            TransitionOffices offices)
        {
            var s = p.StateCode;
            switch (action)
            {
                case ProposalAction.Submit when s == ProposalStateCode.S01:
                case ProposalAction.Resubmit when s == ProposalStateCode.S04:
                {
                    var to = SubmitTarget(p.OriginLevel);
                    return to is null || p.OriginParentOfficeId is null
                        ? null
                        : new ResolvedTransition(s, action, to, p.OriginParentOfficeId.Value);
                }

                case ProposalAction.Recall:
                {
                    // Target = from_state of the last movement, only by the office that made it, and refused once
                    // the office above has opened it (BUD-214, BUD-215).
                    var last = history.Count == 0 ? null : history[^1];
                    if (last is null || last.ToState != s || last.FromOfficeId != actorOfficeId)
                        return null;
                    if (last.ActionCode is not (ProposalAction.Submit or ProposalAction.Resubmit
                        or ProposalAction.Forward or ProposalAction.ForwardFinance))
                        return null;
                    return new ResolvedTransition(s, action, last.FromState, last.FromOfficeId);
                }

                case ProposalAction.Open:
                {
                    var to = s switch
                    {
                        ProposalStateCode.S02 => ProposalStateCode.S03,
                        ProposalStateCode.S05 => ProposalStateCode.S06,
                        ProposalStateCode.S07 => ProposalStateCode.S08,
                        ProposalStateCode.S09 => ProposalStateCode.S10,
                        _ => null,
                    };
                    return to is null ? null : new ResolvedTransition(s, action, to, p.HolderOfficeId);
                }

                case ProposalAction.Return when s is ProposalStateCode.S03 or ProposalStateCode.S06 or ProposalStateCode.S08:
                {
                    // One level down the channel the proposal took up: the from-office of the movement that
                    // brought it to the returning office (design §6.2 rules list, DB-12).
                    var brought = history.LastOrDefault(h => h.ToOfficeId == p.HolderOfficeId && h.ActionCode is
                        ProposalAction.Submit or ProposalAction.Resubmit or ProposalAction.Forward);
                    if (brought is null)
                        return null;
                    var below = brought.FromOfficeId;
                    if (below == p.OriginOfficeId)
                        return new ResolvedTransition(s, action, ProposalStateCode.S04, below);
                    var level = offices.LevelById.TryGetValue(below, out var l) ? l : null;
                    var to = level is null ? null : ExaminationStateOf(level);
                    return to is null ? null : new ResolvedTransition(s, action, to, below);
                }

                case ProposalAction.Forward when s is ProposalStateCode.S03 or ProposalStateCode.S06:
                {
                    var to = s == ProposalStateCode.S03 ? ProposalStateCode.S05 : ProposalStateCode.S07;
                    return p.HolderParentOfficeId is null ? null : new ResolvedTransition(s, action, to, p.HolderParentOfficeId.Value);
                }

                case ProposalAction.ForwardFinance when s == ProposalStateCode.S08:
                    return offices.FinanceOfficeId is null
                        ? null
                        : new ResolvedTransition(s, action, ProposalStateCode.S09, offices.FinanceOfficeId.Value);

                case ProposalAction.SendBack when s == ProposalStateCode.S10:
                    return offices.SecretariatOfficeId is null
                        ? null
                        : new ResolvedTransition(s, action, ProposalStateCode.S08, offices.SecretariatOfficeId.Value);

                case ProposalAction.Finalise when s == ProposalStateCode.S10:
                    return new ResolvedTransition(s, action, ProposalStateCode.S11, p.HolderOfficeId);

                case ProposalAction.Reopen when s == ProposalStateCode.S11:
                    return new ResolvedTransition(s, action, ProposalStateCode.S10, p.HolderOfficeId);

                default:
                    return null;
            }
        }

        /// <summary>The right each transition needs (design §6.2 "Right key"), chosen by category or state.</summary>
        public static string RightFor(string action, string fromState, string category) => action switch
        {
            ProposalAction.Submit or ProposalAction.Resubmit =>
                category == ProposalCategory.Works ? "F07.Submit" : "F11.Submit",
            ProposalAction.Recall => "Proposal.Recall",
            ProposalAction.Open => fromState == ProposalStateCode.S09 ? "Finance.Open" : "Review.Examine",
            ProposalAction.Return or ProposalAction.Forward => "Review.Examine",
            ProposalAction.ForwardFinance => "Review.ForwardFinance",
            ProposalAction.SendBack or ProposalAction.Finalise or ProposalAction.Reopen =>
                category == ProposalCategory.Works ? "F10.Act" : "F13.Act",
            ProposalAction.Freeze => "F20.Freeze",
            _ => "F19.Allocate",
        };
    }
}
