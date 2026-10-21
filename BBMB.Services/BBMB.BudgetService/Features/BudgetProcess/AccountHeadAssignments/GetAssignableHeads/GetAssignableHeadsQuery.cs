using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetAssignableHeads
{
    public sealed record GetAssignableHeadsQuery(long ProposalId) : IRequest<Response<List<AssignedHeadDto>>>;
}
