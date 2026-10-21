using BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.AlterFigures
{
    /// <summary>As Add: <c>Response&lt;SaveDemandLineResponseDto&gt;</c>, <c>Response&lt;RuleFailuresDto&gt;</c> or a plain refusal (incl. ADV-122).</summary>
    public sealed record AlterFiguresCommand(long ProposalId, long DemandLineId, AlterFiguresDto Dto) : IRequest<Response>;
}
