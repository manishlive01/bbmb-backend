using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Forward
{
    /// <summary>Success: the new state; a line rule refusal: <c>Response&lt;RuleFailuresDto&gt;</c>.</summary>
    public sealed record ForwardProposalCommand(long ProposalId, string? Remark) : IRequest<Response>;
}
