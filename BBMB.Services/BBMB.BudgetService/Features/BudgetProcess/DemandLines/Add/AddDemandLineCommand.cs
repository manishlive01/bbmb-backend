using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add
{
    /// <summary>
    /// Success: <c>Response&lt;SaveDemandLineResponseDto&gt;</c>. Refusal by the line rule set:
    /// <c>Response&lt;RuleFailuresDto&gt;</c>. Any other refusal: a plain catalogued <see cref="Response"/>.
    /// </summary>
    public sealed record AddDemandLineCommand(long ProposalId, AddDemandLineDto Dto) : IRequest<Response>;
}
