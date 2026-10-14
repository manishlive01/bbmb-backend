using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Shared.ResponseDTOs;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules
{
    /// <summary>
    /// Hooks a transition slice adds to the common move: a check run inside the action scope before anything is
    /// written (the line rule set of Submit / Forward / Finalise), and work done after the movement row is
    /// written (stamping returned observations with the movement id).
    /// </summary>
    public sealed class TransitionHooks
    {
        /// <summary>Returns a failed response to refuse the move (nothing is written), or null to proceed.</summary>
        public Func<BudgetProposal, CancellationToken, Task<Response?>>? BeforeMove { get; init; }

        /// <summary>Runs after the movement and the proposal update are saved, inside the same transaction.</summary>
        public Func<BudgetProposal, ProposalMovement, CancellationToken, Task>? AfterMove { get; init; }

        public static readonly TransitionHooks None = new();
    }

    /// <summary>The single path by which a proposal moves (design §6.2, §8.1; BUD-234, BUD-235).</summary>
    public interface IProposalTransitionService
    {
        /// <summary>
        /// True once T-16 exists (BudgetP1StateTransitionSeed). A true result is cached for the process lifetime
        /// per database; false is not cached (design §11.4).
        /// </summary>
        Task<bool> IsStateModelAvailableAsync(CancellationToken ct = default);

        Task<Response<ProposalStateResponseDto>> MoveAsync(
            long proposalId, string action, string? remark, CancellationToken ct = default, TransitionHooks? hooks = null);
    }
}
