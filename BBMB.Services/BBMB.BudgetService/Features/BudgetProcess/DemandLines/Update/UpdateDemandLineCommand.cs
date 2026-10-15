using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update
{
    /// <summary>As Add: <c>Response&lt;SaveDemandLineResponseDto&gt;</c>, <c>Response&lt;RuleFailuresDto&gt;</c> or a plain refusal.</summary>
    public sealed record UpdateDemandLineCommand(long ProposalId, long DemandLineId, UpdateDemandLineDto Dto) : IRequest<Response>;
}
