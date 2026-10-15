using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.MarkDeleted
{
    public sealed record MarkDemandLineDeletedCommand(long ProposalId, long DemandLineId) : IRequest<Response>;
}
