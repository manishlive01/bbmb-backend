using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions
{
    public sealed record GetAmrPredecessorOptionsQuery(long ProposalId) : IRequest<Response<List<AmrPredecessorOptionDto>>>;
}
