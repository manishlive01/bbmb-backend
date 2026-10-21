namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetStateModel.Dtos
{
    /// <summary>T-15 states and T-16 transitions (design §8.2 <c>GET /api/budget-states</c>).</summary>
    public sealed class StateModelResponseDto
    {
        /// <summary>False before BudgetP1StateTransitionSeed: <see cref="Transitions"/> is then empty (§11.4).</summary>
        public bool StateModelAvailable { get; set; }
        public List<StateDto> States { get; set; } = [];
        public List<TransitionDto> Transitions { get; set; } = [];
    }

    public sealed record StateDto(string StateCode, string StateName, string HolderLevel, bool FiguresOpen);

    public sealed record TransitionDto(string FromState, string ActionCode, string ToState, bool RemarkRequired);
}
