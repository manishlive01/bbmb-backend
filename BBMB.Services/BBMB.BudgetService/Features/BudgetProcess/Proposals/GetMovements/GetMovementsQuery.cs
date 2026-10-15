using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements
{
    public sealed record GetMovementsQuery(long ProposalId) : IRequest<Response<List<MovementResponseDto>>>;
}
