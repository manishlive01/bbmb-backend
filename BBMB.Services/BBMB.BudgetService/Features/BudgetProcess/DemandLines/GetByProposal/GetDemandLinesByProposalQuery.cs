using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal
{
    /// <param name="Tab">AA, AMR (WORK lines by the work's class), UNFORESEEN or HEAD; null = every line.</param>
    public sealed record GetDemandLinesByProposalQuery(long ProposalId, string? Tab, bool IncludeDeleted, int PageNumber, int PageSize)
        : IRequest<Response<GetDemandLinesResponseDto>>;
}
