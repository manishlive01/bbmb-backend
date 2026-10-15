using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory
{
    public sealed record GetFigureHistoryQuery(long ProposalId, long DemandLineId) : IRequest<Response<List<FigureHistoryItemDto>>>;
}
