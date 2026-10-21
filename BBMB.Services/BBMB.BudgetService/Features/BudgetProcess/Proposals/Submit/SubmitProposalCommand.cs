using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Submit
{
    /// <summary>
    /// Success: <c>Response&lt;ProposalStateResponseDto&gt;</c>. A line rule refusal: <c>Response&lt;RuleFailuresDto&gt;</c>
    /// listing every failing line (BUD-096). Any other refusal: a plain catalogued <see cref="Response"/>.
    /// </summary>
    public sealed record SubmitProposalCommand(long ProposalId) : IRequest<Response>;
}
