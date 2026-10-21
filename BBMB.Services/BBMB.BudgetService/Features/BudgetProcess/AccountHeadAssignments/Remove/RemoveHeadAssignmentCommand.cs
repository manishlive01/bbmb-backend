using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Remove
{
    public sealed record RemoveHeadAssignmentCommand(long AssignmentId) : IRequest<Response>;
}
