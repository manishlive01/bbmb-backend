using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks
{
    /// <summary>The F-30 request actions (design §6.3) that move a T-48 request through its own status model.</summary>
    public static class UnforeseenAction
    {
        public const string Submit = "SUBMIT";
        public const string Forward = "FORWARD";
        public const string Return = "RETURN";
        public const string Resubmit = "RESUBMIT";
        public const string Authorise = "AUTHORISE";
    }

    /// <summary>
    /// The permitted (action → from/to) moves of the unforeseen request status model (design §6.3). The office
    /// level that may act and the remark rule are checked by the handlers; this map fixes the state machine and
    /// the T-49 (from_status, to_status) pairs the DB CHECK also enforces.
    /// </summary>
    public static class UnforeseenMoves
    {
        public sealed record Move(string FromStatus, string ToStatus, bool RemarkRequired);

        private static readonly IReadOnlyDictionary<string, Move> ByAction = new Dictionary<string, Move>
        {
            [UnforeseenAction.Submit] = new(UnforeseenStatus.Open, UnforeseenStatus.AtCircle, false),
            [UnforeseenAction.Forward] = new(UnforeseenStatus.AtCircle, UnforeseenStatus.AtCe, false),
            [UnforeseenAction.Return] = new(UnforeseenStatus.AtCe, UnforeseenStatus.AtCircle, true),
            [UnforeseenAction.Resubmit] = new(UnforeseenStatus.Returned, UnforeseenStatus.AtCircle, false),
        };

        // AT_CIRCLE RETURN RETURNED is a second RETURN move (from the Circle to the Division), distinct from the
        // AT_CE RETURN AT_CIRCLE above; it is resolved by the current status (see ResolveReturn).
        public static Move? For(string action, string currentStatus)
        {
            if (action == UnforeseenAction.Return)
                return ResolveReturn(currentStatus);
            return ByAction.TryGetValue(action, out var move) && move.FromStatus == currentStatus ? move : null;
        }

        private static Move? ResolveReturn(string currentStatus) => currentStatus switch
        {
            UnforeseenStatus.AtCe => new Move(UnforeseenStatus.AtCe, UnforeseenStatus.AtCircle, true),
            UnforeseenStatus.AtCircle => new Move(UnforeseenStatus.AtCircle, UnforeseenStatus.Returned, true),
            _ => null,
        };
    }
}
