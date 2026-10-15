using BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal
{
    public sealed record GetObservationsByProposalQuery(long ProposalId) : IRequest<Response<List<ObservationResponseDto>>>;
}
